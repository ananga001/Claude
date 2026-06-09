---
name: project-calculatormvc-security
description: Security posture, known vulnerabilities, and controls in place for CalculatorMVC — updated after round-1 fix re-scan on 2026-06-09
metadata:
  type: project
---

## Baseline

Full audit completed 2026-06-09. Round-1 fixes applied and re-scanned same day.

**Why:** Full OWASP Top 10 + SAST + API + Dependency audit was requested to baseline the security state of the project after the loan dispersal feature was added (commit 82deca4). A subsequent round of fixes was applied and re-scanned.

**How to apply:** Use this as the current baseline when reviewing future PRs. Do not re-report already-documented and fixed items. Items marked RESOLVED below are confirmed fixed.

## RESOLVED findings (confirmed fixed in re-scan)

- VULN-001 [CRITICAL] SQL Injection — N/A (in-memory store, no SQL); was not applicable
- VULN-002 [HIGH] CSRF — AutoValidateAntiforgeryTokenAttribute now globally registered in Program.cs; asp-* tag helpers confirmed in all forms; Logout POST uses form with token. RESOLVED.
- VULN-003 [HIGH] Role-based access — [Authorize(Roles="Normal/Supervisor/Manager")] added to every queue/approve/reject/disburse action; [Authorize(Roles="Manager")] at UserController class level. RESOLVED.
- VULN-004 [HIGH] Self-registration role elevation — Role dropdown removed from Register.cshtml; UserRole.Normal hardcoded in AccountController. RESOLVED.
- VULN-005 [HIGH] IDOR on Details — ownership check present: isStaff (Manager|Supervisor) OR loan.SubmittedByUsername == User.Identity.Name; returns Forbid() otherwise. RESOLVED.
- VULN-006 [HIGH] AllLoans unrestricted — [Authorize(Roles="Manager,Supervisor")] added. RESOLVED.
- VULN-007 [HIGH] Status guards on Approve/Reject — LoanStore now uses FirstOrDefault with l.Status == LoanStatus.Pending check; returns null if not met. RESOLVED.
- VULN-008 [HIGH] Disburse return value ignored — controller now checks loan == null, logs warning, sets TempData["Error"]. RESOLVED.
- VULN-009 [HIGH] Approve/Reject return values ignored — all three role-pair approve/reject actions now log warning when null returned. RESOLVED.
- VULN-010 [MEDIUM] No rate limiting on auth endpoints — AddFixedWindowLimiter("auth") 10 req/5 min; [EnableRateLimiting("auth")] on Login POST and Register POST. RESOLVED.
- VULN-011 [MEDIUM] XSS via Html.Raw in User/Create.cshtml — replaced with data-* attributes + JSON.parse pattern; no Html.Raw remaining. RESOLVED.
- VULN-012 [MEDIUM] No security headers — X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy, CSP now added via middleware in Program.cs. RESOLVED.
- VULN-013 [MEDIUM] Cookie security flags — HttpOnly=true, SameSite=Strict, ExpireTimeSpan=8h, SlidingExpiration=true now explicitly set. RESOLVED.
- VULN-014 [MEDIUM] No input validation on models — [Required], [MaxLength], [MinLength], [Range], [RegularExpression] added to RegisterViewModel, LoginViewModel, LoanApplication; ModelState.IsValid checked in Login POST and Submit POST. RESOLVED.
- VULN-015 [MEDIUM] Password complexity — [RegularExpression] enforcing upper+lower+digit+special on RegisterViewModel.Password. RESOLVED.
- VULN-016 [LOW] DateTime.Now — replaced with DateTime.UtcNow throughout LoanStore. RESOLVED.
- VULN-017 [LOW] Security logging absent — ILogger injected into AccountController and LoanApprovalController; login success/failure (with IP), registration, logout, and loan state transitions all logged. RESOLVED.
- VULN-018 [LOW] Rejection reason unbounded — capped at 500 chars in all three reject actions. RESOLVED.
- VULN-019 [LOW] SubmittedByUsername not set — set from User.Identity.Name in Submit action (not from form input). RESOLVED.

## STILL OPEN findings (post-fix, confirmed in re-scan 2026-06-09 full-review)

- OPEN-01 [MEDIUM] CookieSecurePolicy.SameAsRequest — cookies are sent over plain HTTP. In a production deployment without TLS termination at the app level this exposes the session cookie to eavesdropping. Should be CookieSecurePolicy.Always. User has not explicitly rejected this yet.
- OPEN-02 [LOW] appsettings.json AllowedHosts="*" — user explicitly rejected fix.
- OPEN-03 [LOW] .gitignore does not exclude appsettings.*.json — user explicitly rejected fix.
- OPEN-04 [LOW] CSP includes 'unsafe-inline' for script-src and style-src — weakens XSS defence; nonce-based CSP would be needed to remove this.
- OPEN-05 [LOW] Rate limit window 10 req/5 min is generous — account enumeration via timing differences is still feasible; consider 5 req/5 min with exponential backoff or CAPTCHA.
- OPEN-06 [LOW] User.cs model has no data annotations — Name has no [Required] or [MaxLength]; mass-assignment risk in UserController.Create POST.
- OPEN-07 [INFORMATIONAL] Details view "All Loans" back-link always rendered — Normal users see the link and receive a 403 when clicking; UX confusion, not a security issue.
- OPEN-08 [INFORMATIONAL] Bootstrap/jQuery loaded from local wwwroot — no SRI needed for local assets; no CDN fallback found.
- OPEN-09 [INFORMATIONAL] LoanApplication.Purpose has no [Required] — empty purpose is accepted; not a security issue but a data quality gap.
- OPEN-10 [MEDIUM] CalculatorController has no [Authorize] — calculator is accessible without authentication; acceptable if intentional but should be a conscious decision.
- OPEN-11 [MEDIUM] HomeController has no [Authorize] — Index, Privacy, Error are public; same note as OPEN-10.
- OPEN-12 [MEDIUM] Any Normal user can approve loans in ANY Normal-queue (no ownership check) — a submitter can approve their own loan by exploiting the role check without a self-approval guard.
- OPEN-13 [MEDIUM] LoanApplication.SubmittedByUsername is a public settable property on the model — mass-assignment could override it if a form is constructed with that field; Submit action sets it from User.Identity.Name AFTER model binding, which is correct, but the model itself has no [BindNever] guard.
- OPEN-14 [LOW] AccountController.Register GET and Login GET have no [Authorize(Policy="...")] to redirect already-authenticated users — minor UX gap (authenticated user can re-visit login page).
- OPEN-15 [INFORMATIONAL] Floating-point used for EMI calculation (double in LoanStore.GenerateRepaymentSchedule) — rounding errors accumulate; decimal arithmetic preferred for financial calculations.

## Security controls confirmed in place (post-fix)

- AutoValidateAntiforgeryTokenAttribute globally registered — all state-changing forms protected
- asp-* tag helpers active via _ViewImports.cshtml — antiforgery tokens automatically injected
- BCrypt.Net-Next for password hashing
- Lock-based thread safety in all in-memory stores
- Role-based authorization on all sensitive actions
- Rate limiting on authentication endpoints
- Security headers middleware (X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy, CSP)
- Cookie hardening: HttpOnly, SameSite=Strict, 8h expiry
- Status-guarded state machine in LoanStore (Pending->Approved/Rejected, Approved->Disbursed)
- IDOR check in Details action
- Structured security logging with IP addresses
- XSS-safe data embedding in User/Create.cshtml
