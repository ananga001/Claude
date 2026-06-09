# .NET Code Quality Subagent

A Claude-powered subagent that reviews your .NET/C# code for quality, maintainability, performance, and best practices.

---

## What It Does

The subagent runs six categories of code quality checks:

| Check | What It Looks For |
|---|---|
| **Clean Code** | Naming conventions, method length, single responsibility, readability |
| **SOLID Principles** | SRP, OCP, LSP, ISP, DIP violations |
| **Performance** | Unnecessary allocations, inefficient LINQ, blocking async calls, memory leaks |
| **Error Handling** | Swallowed exceptions, missing null checks, improper use of try/catch |
| **Testability** | Tight coupling, untestable code, missing interfaces, static abuse |
| **Architecture** | Layer violations, circular dependencies, improper DI usage |

---

## How to Use It

### Option 1 — Paste a Code Snippet

```
Please review this .NET/C# code for quality.

Run all of the following checks:
- Clean code and naming conventions
- SOLID principles
- Performance issues
- Error handling
- Testability
- Architecture and design patterns

For each finding:
1. Category: Clean Code / SOLID / Performance / Error Handling / Testability / Architecture
2. Severity: Critical / High / Medium / Low
3. Explain the issue and its impact
4. Provide a refactored C# code sample showing the improvement

Code:
```csharp
// PASTE YOUR CODE HERE
```
```

---

### Option 2 — Upload Project Files

Upload `.cs`, `.csproj`, or `.sln` files, then send:

```
I have uploaded .NET project files. Please perform a full code quality review.

Run all of the following checks:
- Clean code and naming conventions
- SOLID principles
- Performance issues
- Error handling
- Testability
- Architecture and design patterns

For each finding:
1. Category and severity
2. File and approximate location of the issue
3. Explain why it is a problem
4. Provide a refactored code sample
5. Summarise a prioritised action list at the end
```

---

### Option 3 — GitHub Repository URL

```
Please perform a code quality review on this .NET GitHub repository:
https://github.com/YOUR-ORG/YOUR-REPO

Run all of the following checks:
- Clean code and naming conventions
- SOLID principles
- Performance issues
- Error handling
- Testability
- Architecture and design patterns

For each area:
1. Summary of findings with severity: Critical / High / Medium / Low
2. Specific examples from the codebase
3. Refactored code samples
4. Final prioritised improvement roadmap
```

---

## Understanding the Output

Every finding follows this structure:

```
### [SEVERITY] Finding Title
**Category:** Clean Code / SOLID / Performance / Error Handling / Testability / Architecture

**Issue:**
What the problem is and why it matters.

**Current code:**
```csharp
// Problematic code
```

**Improved code:**
```csharp
// Refactored code
```

**Why this is better:**
Brief explanation of the improvement.
```

### Severity levels

- 🔴 **Critical** — Blocks maintainability or causes runtime risk; fix immediately
- 🟠 **High** — Significant tech debt; fix in current sprint
- 🟡 **Medium** — Noticeable quality issue; fix in next release
- 🟢 **Low** — Minor improvement or stylistic suggestion

---

## Targeted Checks (Optional)

Run a single category when you need focused feedback.

**Clean code only:**
```
Review this C# code for clean code principles only.
Focus on: naming, method length, single responsibility, and readability.
Provide refactored examples for every issue found.
[paste code]
```

**SOLID principles only:**
```
Analyse this C# code for SOLID principle violations.
For each violation state which principle is broken, why it matters,
and show a refactored version that fixes it.
[paste code]
```

**Performance only:**
```
Review this .NET code for performance issues.
Check for: unnecessary allocations, inefficient LINQ, sync-over-async,
missing ConfigureAwait, string concatenation in loops, and N+1 query patterns.
Provide benchmarked alternatives where relevant.
[paste code]
```

**Error handling only:**
```
Review this C# code for error handling quality.
Look for: swallowed exceptions, overly broad catch blocks, missing null checks,
improper use of nullable types, and lack of guard clauses.
[paste code]
```

**Testability only:**
```
Review this C# code for testability.
Identify: tightly coupled dependencies, missing interfaces, static method abuse,
hidden dependencies, and code that is hard to unit test.
Show how to refactor each case to be easily testable with xUnit/NUnit and Moq.
[paste code]
```

---

## Common .NET Quality Issues & Fixes

### Violated Single Responsibility Principle
```csharp
// ❌ One class doing too much
public class OrderService {
    public void PlaceOrder(Order order) { ... }
    public void SendConfirmationEmail(Order order) { ... }
    public void GenerateInvoicePdf(Order order) { ... }
    public void UpdateInventory(Order order) { ... }
}

// ✅ Each class has one reason to change
public class OrderService {
    public void PlaceOrder(Order order) { ... }
}
public class OrderNotificationService {
    public void SendConfirmation(Order order) { ... }
}
public class InvoiceService {
    public void GeneratePdf(Order order) { ... }
}
```

### Sync over Async (Deadlock Risk)
```csharp
// ❌ Blocks the thread, risks deadlock
public User GetUser(int id) {
    return _repository.GetUserAsync(id).Result;
}

// ✅ Properly async all the way
public async Task<User> GetUserAsync(int id) {
    return await _repository.GetUserAsync(id).ConfigureAwait(false);
}
```

### Swallowed Exception
```csharp
// ❌ Exception silently disappears
try {
    ProcessOrder(order);
} catch (Exception) { }

// ✅ Log and handle or rethrow
try {
    ProcessOrder(order);
} catch (Exception ex) {
    _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
    throw;
}
```

### Inefficient LINQ
```csharp
// ❌ Iterates the list multiple times
var count = orders.Where(o => o.IsActive).Count();

// ✅ Single pass
var count = orders.Count(o => o.IsActive);
```

### Missing Null Guard
```csharp
// ❌ NullReferenceException waiting to happen
public string GetCustomerName(Order order) {
    return order.Customer.Name;
}

// ✅ Guard clause at method entry
public string GetCustomerName(Order order) {
    ArgumentNullException.ThrowIfNull(order);
    ArgumentNullException.ThrowIfNull(order.Customer);
    return order.Customer.Name;
}
```

### Tight Coupling (Hard to Test)
```csharp
// ❌ Concrete dependency — cannot be mocked
public class OrderService {
    private readonly SqlOrderRepository _repo = new SqlOrderRepository();
}

// ✅ Inject abstraction — easily mocked in tests
public class OrderService {
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;
}
```

### Magic Numbers
```csharp
// ❌ What does 30 mean?
if (order.DaysSincePlaced > 30) { ... }

// ✅ Named constant with clear intent
private const int OrderExpiryDays = 30;
if (order.DaysSincePlaced > OrderExpiryDays) { ... }
```

---

## Code Quality Metrics to Ask For

Add any of these to your prompt for deeper analysis:

```
Also provide estimates for the following metrics:
- Cyclomatic complexity per method (flag anything above 10)
- Average method length (flag anything above 20 lines)
- Class coupling score
- Test coverage gaps (methods with no obvious test path)
- Technical debt estimate (hours to fix all findings)
```

---

## Refactoring Roadmap Prompt

After an initial audit, use this to get a prioritised plan:

```
Based on the findings from the code quality review, create a refactoring roadmap.

Format it as:
1. Quick wins (under 1 hour each) — list with effort estimate
2. This sprint (1–4 hours each) — list with effort estimate
3. Next sprint (half day to 2 days each) — list with effort estimate
4. Architectural changes (multi-sprint) — list with approach summary

Include the estimated total tech debt hours.
```

---

## Pull Request Review Prompt

Use this for reviewing a specific PR or diff:

```
Please review this C# code diff / pull request for quality.

Focus on:
- Does the new code follow existing patterns in the file?
- Are there any performance regressions?
- Is error handling consistent?
- Is the code testable and are edge cases covered?
- Does anything violate SOLID principles?

Provide inline comments in this format:
[Line/Method]: Issue — Suggested fix

Diff:
[PASTE DIFF HERE]
```

---

## Tips for Best Results

- **Share the full class**, not just the method — SOLID violations are often only visible with full context.
- **Mention your .NET version** (e.g. .NET 8) to get version-appropriate suggestions (e.g. primary constructors, collection expressions).
- **Include related interfaces or base classes** when asking about inheritance or DI issues.
- **Paste your test file alongside the source** to get testability feedback grounded in what you already have.
- **Ask for a summary table** at the end — useful for sharing findings with your team.

---

## Recommended .NET Quality Tools (Complementary)

| Tool | Purpose |
|---|---|
| [Roslyn Analyzers](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) | Built-in .NET static analysis |
| [SonarQube / SonarCloud](https://www.sonarqube.org/) | Continuous code quality monitoring |
| [ReSharper / Rider](https://www.jetbrains.com/resharper/) | Real-time code inspections in VS/Rider |
| [StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) | C# style and consistency enforcement |
| [NDepend](https://www.ndepend.com/) | Architecture and dependency analysis |
| [BenchmarkDotNet](https://benchmarkdotnet.org/) | Micro-benchmarking for performance checks |
| [coverlet](https://github.com/coverlet-coverage/coverlet) | .NET test coverage measurement |

---

*Generated by Claude — Anthropic's AI assistant*