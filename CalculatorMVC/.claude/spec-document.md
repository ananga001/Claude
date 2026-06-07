# Spec Document

## 1. Overview

Chain of Responsibility pattern for loan approval routing in ASP.NET Core MVC.

- Amount < $100 → Normal User approves
- Amount $100–$999 → Supervisor approves
- Amount ≥ $1000 → Manager approves

Loans pass through the chain (Normal → Supervisor → Manager) on submission to determine queue placement.

**Status: Implemented and pushed (commit 24e5900)**

---

## 2. Depends on

Nothing — this is the first step.

---

## 3. Routes

| Route | Controller Action | Purpose |
|---|---|---|
| GET /LoanApproval | Index | Submit loan form |
| POST /LoanApproval/Submit | Submit | Process and queue loan |
| GET /LoanApproval/LoanApproverUser | LoanApproverUser | Normal user queue |
| POST /LoanApproval/UserApprove | UserApprove | Normal user approves |
| GET /LoanApproval/LoanApproverSupervisor | LoanApproverSupervisor | Supervisor queue |
| POST /LoanApproval/SupervisorApprove | SupervisorApprove | Supervisor approves |
| GET /LoanApproval/LoanApproverManager | LoanApproverManager | Manager queue |
| POST /LoanApproval/ManagerApprove | ManagerApprove | Manager approves |

---

## 4. Data Model

```csharp
// Models/LoanApplication.cs
public enum LoanStatus { Pending, Approved }

public class LoanApplication {
    public int Id { get; set; }
    public string ApplicantName { get; set; }
    public string Purpose { get; set; }
    public decimal Amount { get; set; }
    public string CurrentQueue { get; set; }   // "Normal" | "Supervisor" | "Manager"
    public LoanStatus Status { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
```

In-memory singleton store (`Services/LoanStore.cs`) — no database.

---

## 5. Chain of Responsibility

```
Chain/
  LoanApproverBase.cs      ← abstract: SetNext + Handle
  NormalApprover.cs        ← amount < 100 → Normal queue; else pass to Supervisor
  SupervisorApprover.cs    ← amount < 1000 → Supervisor queue; else pass to Manager
  ManagerApprover.cs       ← always → Manager queue
```

Chain runs on submission to set `CurrentQueue`. Each role's queue page shows pending loans for that queue with an Approve button.

---

## 6. Flow

1. User submits name, purpose, and amount via `/LoanApproval`
2. Chain of Responsibility sets `CurrentQueue` based on amount thresholds
3. Each role visits their queue page and approves loans
4. On approval: `Status = Approved`, `ApprovedBy` and `ApprovedAt` are set
5. `EmailService` logs an approval notification (simulated — no SMTP)

## 7. User
Let's add user with different role.
Users for role Normal, Supervisor, Manager
Each user has name , dob, role and reporting people.
Heirarch : Manager has one or more Supervisor. Supervior has one or more normal role user
---

## 7. Key Files

```
Controllers/LoanApprovalController.cs
Models/LoanApplication.cs
Chain/LoanApproverBase.cs
Chain/NormalApprover.cs
Chain/SupervisorApprover.cs
Chain/ManagerApprover.cs
Services/ILoanStore.cs  +  LoanStore.cs
Services/IEmailService.cs  +  EmailService.cs
Views/LoanApproval/Index.cshtml
Views/LoanApproval/LoanApproverUser.cshtml
Views/LoanApproval/LoanApproverSupervisor.cshtml
Views/LoanApproval/LoanApproverManager.cshtml
```
