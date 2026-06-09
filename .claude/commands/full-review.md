Run a full security + code quality review on recently changed or specified files, in strict sequence: SAST security scan first, then code quality review.

## Scope

If the user passed file paths or a description after the command (e.g. `/full-review Controllers/LoanApprovalController.cs`), review those files.  
Otherwise, determine scope automatically:
1. Run `git diff --name-only HEAD` to find files changed in the last commit.
2. Run `git diff --name-only` to find unstaged changes.
3. Combine both lists, deduplicate, and use that as the review scope.
4. If neither produces results, review all `.cs` files under `CalculatorMVC/`.

## Step 1 — SAST Security Scan

Launch the **sast-security-scanner** agent (foreground — wait for it to complete before proceeding).

Pass it this context in the prompt:
- The list of files to scan (from the scope determined above)
- That this is part of a sequential full-review pipeline
- That it should output its full structured report (OWASP A01–A10, Dependency Audit, Secret Detection, SAST, API Security Review)

Do NOT proceed to Step 2 until the sast-security-scanner agent has returned its complete report.

## Step 2 — Code Quality Review

After the security scan completes, launch the **code-quality-reviewer** agent (foreground — wait for it to complete).

Pass it this context in the prompt:
- The same list of files reviewed in Step 1
- That the security scan has already completed (no need to re-check security issues)
- That it should output its full structured report (Clean Code, SOLID, Performance, Error Handling, Testability, Architecture)

## Step 3 — Combined Summary

After both agents have returned their reports, output a combined summary:

```
## Full Review Complete

### Security Scan — Summary
[Copy the CRITICAL/HIGH/MEDIUM/LOW count line from the sast-security-scanner report]
Top 3 security findings (title + severity only):
1. ...
2. ...
3. ...

### Code Quality — Summary
[Copy the CRITICAL/HIGH/MEDIUM/LOW count line from the code-quality-reviewer report]
Top 3 quality findings (title + severity only):
1. ...
2. ...
3. ...

### Combined Priority Action List
Merge the prioritized action lists from both reports, ordered by severity across both:
1. [CRITICAL/HIGH items first, security and quality interleaved by severity]
...

Files reviewed: [list]
```

## Notes

- Run the agents in **strict sequence** (foreground): security first, quality second. Never run them in parallel.
- Each agent reads source files independently — do not pass code snippets between them.
- If a finding appears in both reports (e.g., missing null guard flagged as both a security risk and a quality issue), note it once in the combined summary with both labels.
- If the scope is empty (no changed files, no argument), tell the user and ask them to specify files.
