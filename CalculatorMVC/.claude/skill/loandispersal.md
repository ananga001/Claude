---
name: aspnet-loan-dispersal
description: Extend the existing CalculatorMVC loan approval feature with Reject, Disburse, repayment schedule generation, Details view, and AllLoans view. Use this skill when the user asks to add rejection, disbursement, repayment schedule, loan details, or an all-loans list to the CalculatorMVC loan approval feature. Triggers on: "reject loan", "disburse", "repayment schedule", "loan details", "all loans", or any extension to the existing Chain of Responsibility loan approval system in CalculatorMVC.
---

# CalculatorMVC Loan Dispersal Skill

Extends the **existing** loan approval feature in `CalculatorMVC`. Do not regenerate what already exists — extend it using the same in-memory, synchronous, thread-safe patterns.

## What This Skill Covers

Four extension areas on top of the existing Chain of Responsibility approval flow:
1. **Reject** — per-role reject actions (UserReject, SupervisorReject, ManagerReject) with reason
2. **Disburse** — release funds for manager-queue approved loans
3. **EMI Repayment Schedule** — auto-generated on approval using amortization formula
4. **Details + AllLoans views** — loan detail page with repayment table; all-loans dashboard

---

## Existing Implementation — Do Not Regenerate

**Architecture:** Chain of Responsibility routes submitted loans to role-specific queues. An in-memory singleton store holds all loans. Cookie authentication guards the entire controller.

| File | Namespace | Purpose |
|---|---|---|
| `Controllers/LoanApprovalController.cs` | `CalculatorMVC.Controllers` | `[Authorize]`, 8 actions |
| `Models/LoanApplication.cs` | `CalculatorMVC.Models` | `LoanStatus { Pending, Approved }`, 9 properties |
| `Chain/LoanApproverBase.cs` | `CalculatorMVC.Chain` | Abstract base with `SetNext` / `Handle` |
| `Chain/NormalApprover.cs` | `CalculatorMVC.Chain` | Routes `< $100` → "Normal" queue |
| `Chain/SupervisorApprover.cs` | `CalculatorMVC.Chain` | Routes `$100–$999` → "Supervisor" queue |
| `Chain/ManagerApprover.cs` | `CalculatorMVC.Chain` | Routes `≥ $1000` → "Manager" queue |
| `Services/ILoanStore.cs` | `CalculatorMVC.Services` | `Add`, `GetByQueue`, `Approve` |
| `Services/LoanStore.cs` | `CalculatorMVC.Services` | Singleton, `List<LoanApplication>` with `lock` |
| `Services/IEmailService.cs` | `CalculatorMVC.Services` | `SendApprovalEmail(LoanApplication)` |
| `Services/EmailService.cs` | `CalculatorMVC.Services` | Logs simulated email to ILogger |
| `Views/LoanApproval/Index.cshtml` | — | Submit form (ApplicantName, Purpose, Amount) |
| `Views/LoanApproval/LoanApproverUser.cshtml` | — | Normal queue table (table-dark header) |
| `Views/LoanApproval/LoanApproverSupervisor.cshtml` | — | Supervisor queue table (table-warning header) |
| `Views/LoanApproval/LoanApproverManager.cshtml` | — | Manager queue table (table-danger header) |
| `Program.cs` | — | `AddSingleton<ILoanStore, LoanStore>()`, cookie auth |

**Key facts:**
- No EF Core — pure in-memory `List<LoanApplication>` protected by `lock (_lock)`
- All store methods are synchronous (no `async`/`Task`)
- `[Authorize]` is on the controller class — applies to all actions
- `User.Identity!.Name` returns the logged-in username from the cookie claim

---

## Step 1 — Extend `Models/LoanApplication.cs`

**Add** to the existing `LoanStatus` enum (after `Approved`):
```csharp
Rejected,
Disbursed
```

**Add** these properties to the existing `LoanApplication` class (after `ApprovedAt`):
```csharp
public string? RejectionReason { get; set; }
public string? RejectedBy { get; set; }
public DateTime? RejectedAt { get; set; }
public string? DisbursedBy { get; set; }
public DateTime? DisbursedAt { get; set; }
public List<LoanRepayment> Repayments { get; set; } = new();
```

---

## Step 2 — New `Models/LoanRepayment.cs`

Create this file. No EF navigation properties — `LoanId` is a plain integer cross-reference.

```csharp
namespace CalculatorMVC.Models;

public class LoanRepayment
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal TotalDue { get; set; }
    public decimal Balance { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidDate { get; set; }
}
```

---

## Step 3 — New `Models/ManagerQueueViewModel.cs`

The manager view needs two separate loan lists (pending + approved-awaiting-disbursal). Create this small view model:

```csharp
namespace CalculatorMVC.Models;

public class ManagerQueueViewModel
{
    public IReadOnlyList<LoanApplication> PendingLoans { get; set; } = [];
    public IReadOnlyList<LoanApplication> ApprovedLoans { get; set; } = [];
}
```

---

## Step 4 — Extend `Services/ILoanStore.cs`

**Add** these methods to the existing interface (do not remove the existing three):

```csharp
LoanApplication? Reject(int id, string rejectedBy, string reason);
LoanApplication? Disburse(int id, string disbursedBy);
IReadOnlyList<LoanApplication> GetAll();
LoanApplication? GetById(int id);
```

Full updated interface for reference:
```csharp
namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface ILoanStore
{
    // Existing
    void Add(LoanApplication loan);
    IReadOnlyList<LoanApplication> GetByQueue(string queue);
    LoanApplication? Approve(int id, string approvedBy);

    // New
    LoanApplication? Reject(int id, string rejectedBy, string reason);
    LoanApplication? Disburse(int id, string disbursedBy);
    IReadOnlyList<LoanApplication> GetAll();
    LoanApplication? GetById(int id);
}
```

---

## Step 5 — Extend `Services/LoanStore.cs`

**Modify** the existing `Approve` method to call `GenerateRepaymentSchedule` inside the lock (after setting `ApprovedAt`).

**Add** the new method implementations and the private schedule generator. Full updated class:

```csharp
namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public class LoanStore : ILoanStore
{
    private readonly List<LoanApplication> _loans = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public void Add(LoanApplication loan)
    {
        lock (_lock)
        {
            loan.Id = _nextId++;
            loan.SubmittedAt = DateTime.Now;
            _loans.Add(loan);
        }
    }

    public IReadOnlyList<LoanApplication> GetByQueue(string queue)
    {
        lock (_lock)
            return _loans.Where(l => l.CurrentQueue == queue && l.Status == LoanStatus.Pending).ToList();
    }

    public LoanApplication? Approve(int id, string approvedBy)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return null;
            loan.Status = LoanStatus.Approved;
            loan.ApprovedBy = approvedBy;
            loan.ApprovedAt = DateTime.Now;
            GenerateRepaymentSchedule(loan);
            return loan;
        }
    }

    public LoanApplication? Reject(int id, string rejectedBy, string reason)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return null;
            loan.Status = LoanStatus.Rejected;
            loan.RejectedBy = rejectedBy;
            loan.RejectionReason = reason;
            loan.RejectedAt = DateTime.Now;
            return loan;
        }
    }

    public LoanApplication? Disburse(int id, string disbursedBy)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id && l.Status == LoanStatus.Approved);
            if (loan is null) return null;
            loan.Status = LoanStatus.Disbursed;
            loan.DisbursedBy = disbursedBy;
            loan.DisbursedAt = DateTime.Now;
            return loan;
        }
    }

    public IReadOnlyList<LoanApplication> GetAll()
    {
        lock (_lock) return _loans.ToList();
    }

    public LoanApplication? GetById(int id)
    {
        lock (_lock) return _loans.FirstOrDefault(l => l.Id == id);
    }

    // EMI amortization schedule. Defaults: 12 months, 10% annual.
    // The existing form does not collect TermMonths or InterestRate.
    // To make these configurable: add TermMonths (int) and AnnualInterestRatePercent (double)
    // to LoanApplication and add those fields to Views/LoanApproval/Index.cshtml.
    private static void GenerateRepaymentSchedule(LoanApplication loan)
    {
        const int termMonths = 12;
        const double annualRatePercent = 10.0;

        double monthlyRate = annualRatePercent / 100.0 / 12.0;
        double principal = (double)loan.Amount;
        double emi = principal * monthlyRate * Math.Pow(1 + monthlyRate, termMonths)
                     / (Math.Pow(1 + monthlyRate, termMonths) - 1);

        double balance = principal;
        var startDate = loan.ApprovedAt ?? DateTime.Now;

        for (int i = 1; i <= termMonths; i++)
        {
            double interest = balance * monthlyRate;
            double principalPart = emi - interest;
            balance -= principalPart;

            loan.Repayments.Add(new LoanRepayment
            {
                Id = i,
                LoanId = loan.Id,
                InstallmentNumber = i,
                DueDate = startDate.AddMonths(i),
                Principal = (decimal)Math.Round(principalPart, 2),
                Interest = (decimal)Math.Round(interest, 2),
                TotalDue = (decimal)Math.Round(emi, 2),
                Balance = (decimal)Math.Round(Math.Max(balance, 0), 2)
            });
        }
    }
}
```

---

## Step 6 — Extend `Controllers/LoanApprovalController.cs`

**Fix** the three existing Approve actions — replace hardcoded role strings with `User.Identity!.Name`:
- `UserApprove`: `_store.Approve(id, User.Identity!.Name!)` (was `"Normal User"`)
- `SupervisorApprove`: `_store.Approve(id, User.Identity!.Name!)` (was `"Supervisor"`)
- `ManagerApprove`: `_store.Approve(id, User.Identity!.Name!)` (was `"Manager"`)

**Change** `LoanApproverManager()` to use the new view model:
```csharp
public IActionResult LoanApproverManager()
{
    var vm = new ManagerQueueViewModel
    {
        PendingLoans = _store.GetByQueue("Manager"),
        ApprovedLoans = _store.GetAll()
                              .Where(l => l.CurrentQueue == "Manager"
                                       && l.Status == LoanStatus.Approved)
                              .ToList()
    };
    return View(vm);
}
```

**Add** these new actions to the controller:

```csharp
[HttpPost]
public IActionResult UserReject(int id, string reason)
{
    _store.Reject(id, User.Identity!.Name!, reason);
    return RedirectToAction("LoanApproverUser");
}

[HttpPost]
public IActionResult SupervisorReject(int id, string reason)
{
    _store.Reject(id, User.Identity!.Name!, reason);
    return RedirectToAction("LoanApproverSupervisor");
}

[HttpPost]
public IActionResult ManagerReject(int id, string reason)
{
    _store.Reject(id, User.Identity!.Name!, reason);
    return RedirectToAction("LoanApproverManager");
}

[HttpPost]
public IActionResult Disburse(int id)
{
    _store.Disburse(id, User.Identity!.Name!);
    return RedirectToAction("LoanApproverManager");
}

public IActionResult Details(int id)
{
    var loan = _store.GetById(id);
    if (loan is null) return NotFound();
    return View(loan);
}

public IActionResult AllLoans()
    => View(_store.GetAll());
```

---

## Step 7 — Update Queue Views (add Reject button)

Replace the `<td>` Action column in each queue view. Shown per view because action names and redirect targets differ.

### `LoanApproverUser.cshtml` — Action `<td>`:
```html
<td>
    <form asp-action="UserApprove" method="post" class="d-inline">
        <input type="hidden" name="id" value="@loan.Id" />
        <button type="submit" class="btn btn-success btn-sm">Approve</button>
    </form>
    <form asp-action="UserReject" method="post" class="d-inline ms-2">
        <input type="hidden" name="id" value="@loan.Id" />
        <input type="text" name="reason" placeholder="Rejection reason"
               class="form-control form-control-sm d-inline w-auto" required />
        <button type="submit" class="btn btn-danger btn-sm"
                onclick="return confirm('Reject this loan?')">Reject</button>
    </form>
</td>
```

### `LoanApproverSupervisor.cshtml` — Action `<td>`:
```html
<td>
    <form asp-action="SupervisorApprove" method="post" class="d-inline">
        <input type="hidden" name="id" value="@loan.Id" />
        <button type="submit" class="btn btn-success btn-sm">Approve</button>
    </form>
    <form asp-action="SupervisorReject" method="post" class="d-inline ms-2">
        <input type="hidden" name="id" value="@loan.Id" />
        <input type="text" name="reason" placeholder="Rejection reason"
               class="form-control form-control-sm d-inline w-auto" required />
        <button type="submit" class="btn btn-danger btn-sm"
                onclick="return confirm('Reject this loan?')">Reject</button>
    </form>
</td>
```

### `LoanApproverManager.cshtml` — full rewrite (now uses `ManagerQueueViewModel`):
```html
@model CalculatorMVC.Models.ManagerQueueViewModel
@{
    ViewData["Title"] = "Manager Queue";
}

<div class="container mt-5">
    <h2 class="mb-1">Manager — Loan Queue</h2>
    <p class="text-muted mb-4">Loans $1000 and above.</p>

    <h5>Pending Approval</h5>
    @if (!Model.PendingLoans.Any())
    {
        <div class="alert alert-info">No pending loans in this queue.</div>
    }
    else
    {
        <table class="table table-bordered table-hover">
            <thead class="table-danger">
                <tr>
                    <th>#</th><th>Applicant</th><th>Purpose</th>
                    <th>Amount</th><th>Submitted</th><th>Action</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var loan in Model.PendingLoans)
                {
                    <tr>
                        <td>@loan.Id</td>
                        <td>@loan.ApplicantName</td>
                        <td>@loan.Purpose</td>
                        <td>$@loan.Amount.ToString("0.00")</td>
                        <td>@loan.SubmittedAt.ToString("g")</td>
                        <td>
                            <form asp-action="ManagerApprove" method="post" class="d-inline">
                                <input type="hidden" name="id" value="@loan.Id" />
                                <button type="submit" class="btn btn-success btn-sm">Approve</button>
                            </form>
                            <form asp-action="ManagerReject" method="post" class="d-inline ms-2">
                                <input type="hidden" name="id" value="@loan.Id" />
                                <input type="text" name="reason" placeholder="Rejection reason"
                                       class="form-control form-control-sm d-inline w-auto" required />
                                <button type="submit" class="btn btn-danger btn-sm"
                                        onclick="return confirm('Reject this loan?')">Reject</button>
                            </form>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    <h5 class="mt-4">Approved — Awaiting Disbursement</h5>
    @if (!Model.ApprovedLoans.Any())
    {
        <div class="alert alert-info">No approved loans awaiting disbursement.</div>
    }
    else
    {
        <table class="table table-bordered table-hover">
            <thead class="table-success">
                <tr>
                    <th>#</th><th>Applicant</th><th>Purpose</th>
                    <th>Amount</th><th>Approved By</th><th>Action</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var loan in Model.ApprovedLoans)
                {
                    <tr>
                        <td>@loan.Id</td>
                        <td>@loan.ApplicantName</td>
                        <td>@loan.Purpose</td>
                        <td>$@loan.Amount.ToString("0.00")</td>
                        <td>@loan.ApprovedBy</td>
                        <td>
                            <form asp-action="Disburse" method="post" class="d-inline">
                                <input type="hidden" name="id" value="@loan.Id" />
                                <button type="submit" class="btn btn-primary btn-sm"
                                        onclick="return confirm('Disburse funds for this loan?')">Disburse</button>
                            </form>
                            <a asp-action="Details" asp-route-id="@loan.Id"
                               class="btn btn-info btn-sm ms-1">Details</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
</div>
```

---

## Step 8 — New `Views/LoanApproval/Details.cshtml`

```html
@model CalculatorMVC.Models.LoanApplication
@{
    ViewData["Title"] = $"Loan #{Model.Id}";
}

<div class="container mt-5">
    <h2 class="mb-1">Loan #@Model.Id — @Model.ApplicantName</h2>
    <p>
        <span class="badge bg-@StatusColor(Model.Status) fs-6">@Model.Status</span>
    </p>

    <dl class="row mt-3">
        <dt class="col-sm-3">Applicant</dt>
        <dd class="col-sm-9">@Model.ApplicantName</dd>

        <dt class="col-sm-3">Purpose</dt>
        <dd class="col-sm-9">@Model.Purpose</dd>

        <dt class="col-sm-3">Amount</dt>
        <dd class="col-sm-9">$@Model.Amount.ToString("0.00")</dd>

        <dt class="col-sm-3">Queue</dt>
        <dd class="col-sm-9">@Model.CurrentQueue</dd>

        <dt class="col-sm-3">Submitted</dt>
        <dd class="col-sm-9">@Model.SubmittedAt.ToString("dd MMM yyyy HH:mm")</dd>

        @if (Model.ApprovedAt.HasValue)
        {
            <dt class="col-sm-3">Approved By</dt>
            <dd class="col-sm-9">@Model.ApprovedBy on @Model.ApprovedAt.Value.ToString("dd MMM yyyy HH:mm")</dd>
        }

        @if (Model.Status == CalculatorMVC.Models.LoanStatus.Rejected)
        {
            <dt class="col-sm-3">Rejected By</dt>
            <dd class="col-sm-9">@Model.RejectedBy on @Model.RejectedAt?.ToString("dd MMM yyyy HH:mm")</dd>

            <dt class="col-sm-3">Reason</dt>
            <dd class="col-sm-9 text-danger">@Model.RejectionReason</dd>
        }

        @if (Model.DisbursedAt.HasValue)
        {
            <dt class="col-sm-3">Disbursed By</dt>
            <dd class="col-sm-9">@Model.DisbursedBy on @Model.DisbursedAt.Value.ToString("dd MMM yyyy HH:mm")</dd>
        }
    </dl>

    @if (Model.Repayments.Any())
    {
        <h4 class="mt-4">Repayment Schedule (12 months @ 10% p.a.)</h4>
        <table class="table table-sm table-bordered">
            <thead class="table-light">
                <tr>
                    <th>#</th><th>Due Date</th><th>Principal</th>
                    <th>Interest</th><th>EMI</th><th>Balance</th><th>Paid</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var r in Model.Repayments.OrderBy(r => r.InstallmentNumber))
                {
                    <tr class="@(r.IsPaid ? "table-success" : "")">
                        <td>@r.InstallmentNumber</td>
                        <td>@r.DueDate.ToString("dd MMM yyyy")</td>
                        <td>$@r.Principal.ToString("0.00")</td>
                        <td>$@r.Interest.ToString("0.00")</td>
                        <td>$@r.TotalDue.ToString("0.00")</td>
                        <td>$@r.Balance.ToString("0.00")</td>
                        <td>@(r.IsPaid ? "Yes" : "—")</td>
                    </tr>
                }
            </tbody>
        </table>
    }

    <a asp-action="AllLoans" class="btn btn-secondary mt-3">All Loans</a>
</div>

@functions {
    string StatusColor(CalculatorMVC.Models.LoanStatus s) => s switch {
        CalculatorMVC.Models.LoanStatus.Pending   => "secondary",
        CalculatorMVC.Models.LoanStatus.Approved  => "success",
        CalculatorMVC.Models.LoanStatus.Rejected  => "danger",
        CalculatorMVC.Models.LoanStatus.Disbursed => "primary",
        _ => "secondary"
    };
}
```

---

## Step 9 — New `Views/LoanApproval/AllLoans.cshtml`

```html
@model IEnumerable<CalculatorMVC.Models.LoanApplication>
@{
    ViewData["Title"] = "All Loans";
}

<div class="container mt-5">
    <h2 class="mb-4">All Loan Applications</h2>

    @if (!Model.Any())
    {
        <div class="alert alert-info">No loans submitted yet.</div>
    }
    else
    {
        <table class="table table-bordered table-hover">
            <thead class="table-light">
                <tr>
                    <th>#</th><th>Applicant</th><th>Purpose</th><th>Amount</th>
                    <th>Queue</th><th>Status</th><th>Submitted</th><th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var loan in Model.OrderByDescending(l => l.SubmittedAt))
                {
                    <tr>
                        <td>@loan.Id</td>
                        <td>@loan.ApplicantName</td>
                        <td>@loan.Purpose</td>
                        <td>$@loan.Amount.ToString("0.00")</td>
                        <td>@loan.CurrentQueue</td>
                        <td>
                            <span class="badge bg-@StatusColor(loan.Status)">@loan.Status</span>
                        </td>
                        <td>@loan.SubmittedAt.ToString("dd MMM yyyy")</td>
                        <td>
                            <a asp-action="Details" asp-route-id="@loan.Id"
                               class="btn btn-info btn-sm">Details</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
</div>

@functions {
    string StatusColor(CalculatorMVC.Models.LoanStatus s) => s switch {
        CalculatorMVC.Models.LoanStatus.Pending   => "secondary",
        CalculatorMVC.Models.LoanStatus.Approved  => "success",
        CalculatorMVC.Models.LoanStatus.Rejected  => "danger",
        CalculatorMVC.Models.LoanStatus.Disbursed => "primary",
        _ => "secondary"
    };
}
```

---

## Key Notes for Claude

1. **No EF Core.** No `DbContext`, no migrations, no NuGet packages. Store is a plain `List<T>` with `lock`.

2. **No new `Program.cs` line.** `ILoanStore` is already registered as `AddSingleton`. `LoanRepayment` is a plain model — not a service.

3. **`User.Identity!.Name`** returns the logged-in username from the cookie (`ClaimTypes.Name`), set during login in `AccountController`. It is not the `User` model's display name.

4. **`[Authorize]` is class-level** — do not add per-action attributes unless introducing role restrictions.

5. **Disburse guard is in the store** — `LoanStore.Disburse` filters `l.Status == LoanStatus.Approved`. The controller does not need to re-check.

6. **Repayment defaults** — 12 months at 10% annual because `Index.cshtml` has no term/rate fields. To make configurable: add `TermMonths int` and `AnnualInterestRatePercent double` to `LoanApplication`, add form fields to `Index.cshtml`, and pass them into `GenerateRepaymentSchedule`.

7. **Bootstrap 5** is already in the shared layout. Do not add CDN links.

8. **No `[ValidateAntiForgeryToken]`** — not currently used in the controller; do not introduce it.

9. **Rejection email** — if needed, add `SendRejectionEmail(LoanApplication loan)` to `IEmailService`. Do not overload `SendApprovalEmail`.

10. **Implementation order** — always follow: `LoanRepayment.cs` → `LoanApplication.cs` → `ManagerQueueViewModel.cs` → `ILoanStore.cs` → `LoanStore.cs` → `LoanApprovalController.cs` → views. Later types depend on earlier ones.
