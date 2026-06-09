---
name: feedback-calculatormvc-antipatterns
description: Recurring insecure patterns in CalculatorMVC — updated after round-1 fixes confirmed resolved in re-scan 2026-06-09
metadata:
  type: feedback
---

Recurring insecure patterns found across CalculatorMVC.

**Why:** These patterns appeared in multiple places in the original audit. The round-1 fix pass resolved the major ones. Remaining patterns should guide review focus on future features.

**How to apply:** When reviewing new controllers, models, or views, check these patterns first.

## Pattern 1 — No [ValidateAntiForgeryToken] (RESOLVED)
All POST actions lacked [ValidateAntiForgeryToken]. Fixed by adding AutoValidateAntiforgeryTokenAttribute globally in Program.cs. Do not use [IgnoreAntiforgeryToken] on any new state-changing action.

## Pattern 2 — No role enforcement inside [Authorize] (RESOLVED)
[Authorize] without Roles= allowed any authenticated user to reach role-specific queues. Fixed by adding Roles= constraint to every action. Future actions must specify the minimum required role.

## Pattern 3 — No model-level data annotations (PARTIALLY RESOLVED)
RegisterViewModel, LoginViewModel, and LoanApplication now have validation attributes. However, the User domain model (User.cs) still has no annotations — Name has no [Required] or [MaxLength]. Any new domain model must carry annotations before being used as an action parameter.

## Pattern 4 — AllowedHosts wildcard (USER DECLINED FIX)
appsettings.json "AllowedHosts": "*" — user explicitly declined to fix. Do not re-raise this unless the deployment environment changes to require it.

## Pattern 5 — Html.Raw with server-side data in JS blocks (RESOLVED)
User/Create.cshtml now uses data-* attributes and JSON.parse. Do not use Html.Raw() for server-side data in any new view.

## Pattern 6 — No status-guard on state-transition mutations (RESOLVED)
LoanStore.Approve/Reject now guard on l.Status == LoanStatus.Pending. LoanStore.Disburse guards on l.Status == LoanStatus.Approved. Future store mutations must guard on the required precondition status.

## Pattern 7 — Silent failure on invalid state transitions (RESOLVED)
All three approve/reject/disburse actions now check the return value and log a warning or set TempData["Error"] on null.

## Pattern 8 — CookieSecurePolicy.SameAsRequest (STILL OPEN)
Cookie SecurePolicy is set to SameAsRequest instead of Always. This means the auth cookie will be sent over HTTP if the app is accessed without TLS. Fix: change to CookieSecurePolicy.Always and ensure HTTPS is enforced end-to-end.

## Pattern 9 — unsafe-inline in CSP (STILL OPEN)
The CSP header includes 'unsafe-inline' for both script-src and style-src. This nullifies XSS protection from CSP. The root cause is that Bootstrap and inline Razor scripts are used without nonces. Fixing requires either nonce injection middleware or moving all inline scripts to external files.
