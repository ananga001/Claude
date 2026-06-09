---
name: project-recurring-patterns
description: Recurring code quality anti-patterns and confirmed good practices in the CalculatorMVC codebase
metadata:
  type: project
---

## Confirmed Good Practices (preserve)
- All three singleton stores (UserStore, AccountStore, LoanStore) use `lock (_lock)` with a plain `List<T>` — correct for this concurrency model
- `AutoValidateAntiforgeryTokenAttribute` applied globally in `Program.cs` — no per-action decoration needed
- Rate limiting applied to auth endpoints via `[EnableRateLimiting("auth")]`
- DI lifetimes all correct: all in-memory stores registered as Singleton in Program.cs
- Chain of Responsibility: `SetNext` returns `next`, enabling fluent chaining; each approver either sets queue or delegates to `_next?.Handle()` — propagation is correct
- Interfaces exist for every service (ILoanStore, IUserStore, IAccountStore, IEmailService)
- Password hashing uses BCrypt.Net-Next; no plaintext passwords stored

## Recurring Anti-Patterns Found (2026-06-09 review)
- **Magic number 500** (max rejection reason length) appears independently in UserReject, SupervisorReject, ManagerReject — should be a named constant
- **`DateTime.UtcNow` used directly** in LoanStore.Add, Approve, Reject, Disburse — not injectable; blocks deterministic testing
- **`StatusColor()` helper duplicated** verbatim in Details.cshtml and AllLoans.cshtml — should be in a shared partial or tag helper
- **N+1 look-up in UserController.Index** — builds ReportingToNames dictionary by calling `_store.GetById()` per user inside a LINQ expression; GetAll() is already called, so a dictionary from that result is the fix
- **Model mutation as side-effect in Chain** — NormalApprover/SupervisorApprover/ManagerApprover mutate `loan.CurrentQueue` directly rather than returning a result; acceptable for this pattern but worth noting
- **`[ValidateAntiForgeryToken]` not present on Logout POST** — global filter covers it, but Logout has no `[HttpPost]`; however global AutoValidateAntiforgery does handle this
- **CalculatorController has no `[ValidateAntiForgeryToken]`** — Calculate POST is not protected (global AutoValidateAntiforgery applies, so this is covered, but controller has no `[Authorize]`)
- **`LoanRepayment.IsPaid` defaults to `false` redundantly** — `= false` is the C# default for bool
- **EMI calculation uses `double` arithmetic** for financial amounts and then casts to `decimal` — rounding errors accumulate across 12 installments

**Why:** These are the first patterns to check in future sessions on this codebase.
**How to apply:** When reviewing new features in LoanApproval or Store services, scan for these same patterns appearing again.
