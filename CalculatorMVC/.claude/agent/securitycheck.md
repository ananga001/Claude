# .NET Security Subagent

A Claude-powered subagent that performs comprehensive security audits on .NET/C# applications. Supports code snippets, uploaded files, and GitHub repositories.

---

## What It Does

The subagent runs five categories of security checks on your .NET code:

| Check | What It Looks For |
|---|---|
| **OWASP Top 10** | SQL injection, XSS, broken auth, insecure deserialization, SSRF, etc. |
| **Dependency Audit** | NuGet packages with known CVEs |
| **Secret Detection** | Hardcoded API keys, connection strings, passwords, tokens |
| **SAST (Static Analysis)** | Unsafe code patterns, null ref risks, unvalidated inputs |
| **API Security Review** | Missing auth, overly permissive CORS, rate limiting gaps |

---

## How to Use It

### Option 1 — Paste a Code Snippet

Copy your prompt below and replace the placeholder with your C# code:

```
Please perform a .NET security audit on this code.

Run all of the following checks:
- OWASP Top 10
- Dependency audit (NuGet packages with known CVEs)
- Secret / credential detection
- SAST static analysis
- API security review

For each finding:
1. Severity: Critical / High / Medium / Low
2. What the vulnerability is and why it is risky
3. A corrected C# code sample showing the fix

Code:
```csharp
// PASTE YOUR CODE HERE
```
```

---

### Option 2 — Upload Project Files

Upload one or more `.cs`, `.csproj`, `.sln`, `.json`, or `.config` files, then send this prompt:

```
I have uploaded .NET project files. Please perform a full security audit.

Run all of the following checks:
- OWASP Top 10
- Dependency audit (NuGet packages with known CVEs)
- Secret / credential detection
- SAST static analysis
- API security review

For each finding:
1. Severity: Critical / High / Medium / Low
2. Explain the vulnerability and its attack surface
3. Provide a corrected .NET/C# code sample
4. Highlight any hardcoded secrets, outdated NuGet packages with CVEs, or OWASP violations
```

---

### Option 3 — GitHub Repository URL

Send this prompt with your repo URL (public repos only):

```
Please perform a .NET security audit on this GitHub repository:
https://github.com/YOUR-ORG/YOUR-REPO

Run all of the following checks:
- OWASP Top 10
- Dependency audit (NuGet packages with known CVEs)
- Secret / credential detection
- SAST static analysis
- API security review

For each area:
1. Summarise key findings with severity: Critical / High / Medium / Low
2. Explain each vulnerability and its risk
3. Provide remediation guidance with corrected .NET/C# code samples where applicable
4. Flag any hardcoded secrets, insecure dependencies, or OWASP Top 10 violations
```

---

## Understanding the Output

Every finding follows this structure:

```
### [SEVERITY] Finding Title
**Check:** OWASP Top 10 / Dependency Audit / Secret Detection / SAST / API Security

**Issue:**
Description of the vulnerability and why it is dangerous.

**Vulnerable code:**
```csharp
// The problematic code
```

**Fixed code:**
```csharp
// The corrected code
```
```

### Severity levels

- 🔴 **Critical** — Exploitable immediately; fix before deployment
- 🟠 **High** — Significant risk; fix in the current sprint
- 🟡 **Medium** — Should be addressed in the next release
- 🟢 **Low** — Best practice improvement; address when convenient

---

## Targeted Checks (Optional)

To run only specific checks, list them explicitly in your prompt. Examples:

**OWASP only:**
```
Audit this C# controller for OWASP Top 10 vulnerabilities only.
List findings by severity and provide fixed code samples.
[paste code]
```

**Secrets only:**
```
Scan this .NET project for hardcoded secrets, API keys, passwords,
and connection strings. Flag every occurrence with file and line context.
[paste code or upload files]
```

**Dependency audit only:**
```
Review the NuGet packages in this .csproj / packages.config file
for known CVEs. For each vulnerable package, state the CVE ID,
severity, and the safe version to upgrade to.
[paste file contents]
```

---

## Common .NET Vulnerabilities to Watch

### SQL Injection
```csharp
// ❌ Vulnerable
var query = "SELECT * FROM Users WHERE Id = " + userId;

// ✅ Fixed — use parameterised queries
var query = "SELECT * FROM Users WHERE Id = @userId";
cmd.Parameters.AddWithValue("@userId", userId);
```

### Hardcoded Secrets
```csharp
// ❌ Vulnerable
var apiKey = "sk-live-abc123xyz";

// ✅ Fixed — read from environment or IConfiguration
var apiKey = _configuration["ExternalApi:Key"];
```

### Missing Authorisation
```csharp
// ❌ Vulnerable
[HttpGet("/admin/users")]
public IActionResult GetAllUsers() { ... }

// ✅ Fixed
[Authorize(Roles = "Admin")]
[HttpGet("/admin/users")]
public IActionResult GetAllUsers() { ... }
```

### Insecure CORS
```csharp
// ❌ Vulnerable
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ✅ Fixed
builder.Services.AddCors(options => {
    options.AddPolicy("Strict", p =>
        p.WithOrigins("https://yourdomain.com")
         .WithMethods("GET", "POST")
         .WithHeaders("Content-Type", "Authorization"));
});
```

---

## Tips for Best Results

- **Include the full controller or service class** rather than isolated methods — context matters for auth and input validation checks.
- **Share your `packages.config` or `*.csproj`** alongside code files for accurate dependency auditing.
- **Mention your .NET version** (e.g. .NET 8, .NET Framework 4.8) if you want version-specific guidance.
- **For private repos**, copy and paste the relevant files rather than sharing a URL.
- **Run checks iteratively** — start with Critical and High findings, fix them, then re-audit.

---

## Recommended .NET Security Tools (Complementary)

| Tool | Purpose |
|---|---|
| [dotnet-retire](https://github.com/RetireNet/dotnet-retire) | CLI NuGet vulnerability scanner |
| [Security Code Scan](https://security-code-scan.github.io/) | Roslyn-based SAST for Visual Studio |
| [OWASP Dependency-Check](https://owasp.org/www-project-dependency-check/) | CVE scanning for NuGet packages |
| [Microsoft Threat Modeling Tool](https://aka.ms/tmt) | Architecture-level threat modelling |
| [Snyk for .NET](https://snyk.io/docs/snyk-for-dotnet/) | Continuous dependency monitoring |

---

*Generated by Claude — Anthropic's AI assistant*