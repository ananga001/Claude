# Spec: User Registration & Login

## 1. Overview

Add cookie-based authentication to CalculatorMVC so that only registered and
logged-in users can submit loan applications. Unauthenticated users are
redirected to the Login page.

- Any visitor can register an account (username + password + role)
- Registered users can log in and receive an auth cookie
- `/LoanApproval` submit actions require authentication
- Approver queue pages (Normal / Supervisor / Manager) are also protected
- Logout clears the cookie

---

## 2. Depends on

- User Management feature (User model, UserRole enum, UserStore) — already implemented
- Existing LoanApproval routes — already implemented

---

## 3. Routes

| Route | Controller Action | Auth Required | Purpose |
|---|---|---|---|
| GET  /Account/Register | Register (GET) | No | Show registration form |
| POST /Account/Register | Register (POST) | No | Create account, auto-login, redirect to home |
| GET  /Account/Login | Login (GET) | No | Show login form |
| POST /Account/Login | Login (POST) | No | Validate credentials, issue cookie |
| POST /Account/Logout | Logout | Yes | Clear cookie, redirect to Login |
| GET  /LoanApproval | Index | **Yes** | Submit loan form |
| POST /LoanApproval/Submit | Submit | **Yes** | Process and queue loan |
| GET  /LoanApproval/LoanApproverUser | LoanApproverUser | **Yes** | Normal user queue |
| POST /LoanApproval/UserApprove | UserApprove | **Yes** | Approve loan |
| GET  /LoanApproval/LoanApproverSupervisor | LoanApproverSupervisor | **Yes** | Supervisor queue |
| POST /LoanApproval/SupervisorApprove | SupervisorApprove | **Yes** | Approve loan |
| GET  /LoanApproval/LoanApproverManager | LoanApproverManager | **Yes** | Manager queue |
| POST /LoanApproval/ManagerApprove | ManagerApprove | **Yes** | Approve loan |

---

## 4. Data Model

```csharp
// Models/Account.cs
public class Account
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // BCrypt hash
    public UserRole Role { get; set; }
}
```

Separate from the `User` hierarchy model. Stored in in-memory `AccountStore`
singleton — no database.

Password hashing: use `BCrypt.Net-Next` NuGet package (`BCrypt.Net.BCrypt.HashPassword` /
`BCrypt.Net.BCrypt.Verify`).

---

## 5. Services

```
Services/
  IAccountStore.cs    ← Register(Account), FindByUsername(string), ValidatePassword(username, password)
  AccountStore.cs     ← in-memory List<Account> with lock, mirrors LoanStore pattern
```

`ValidatePassword` finds the account by username and calls `BCrypt.Verify`.

---

## 6. Authentication Setup (`Program.cs`)

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath   = "/Account/Login";
        options.LogoutPath  = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddSingleton<IAccountStore, AccountStore>();

// After builder.Build():
app.UseAuthentication();
app.UseAuthorization();
```

---

## 7. Controller: `AccountController`

```
Controllers/AccountController.cs
```

**Register (GET)** — return `View(new RegisterViewModel())`

**Register (POST)**
1. Check username not already taken → ModelState error if duplicate
2. Hash password with BCrypt
3. `_accountStore.Register(account)`
4. Sign in: `HttpContext.SignInAsync(...)` with claims (Name = username, Role = role)
5. Redirect to `/LoanApproval`

**Login (GET)** — return `View(new LoginViewModel())`

**Login (POST)**
1. `_accountStore.ValidatePassword(username, password)` → null if invalid
2. On failure: `ModelState.AddModelError("", "Invalid username or password.")`
3. On success: `HttpContext.SignInAsync(...)` → redirect to `/LoanApproval`

**Logout (POST)**
1. `HttpContext.SignOutAsync()`
2. Redirect to `/Account/Login`

---

## 8. View Models

```csharp
// Models/RegisterViewModel.cs
public class RegisterViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public UserRole Role { get; set; }
}

// Models/LoginViewModel.cs
public class LoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
```

---

## 9. Views

```
Views/Account/
  Register.cshtml   ← Bootstrap form: Username, Password, Confirm Password, Role dropdown
  Login.cshtml      ← Bootstrap form: Username, Password, Login button + "Register" link
```

---

## 10. Protect LoanApproval

Add `[Authorize]` attribute to `LoanApprovalController` class (covers all actions):

```csharp
[Authorize]
public class LoanApprovalController : Controller { ... }
```

---

## 11. Navigation

Update `_Layout.cshtml`:
- Show **Login** / **Register** links when not authenticated
- Show **Logout** button and logged-in username when authenticated
- Use `User.Identity!.IsAuthenticated` to toggle

---

## 12. Flow

```
Visitor → GET /LoanApproval → redirected to /Account/Login
           → GET /Account/Register → fill form → POST /Account/Register
           → auto-logged-in → redirected to /LoanApproval
           → submits loan → Chain of Responsibility queues it
           → POST /Account/Logout → back to /Account/Login
```

---

## 13. Key Files

```
Models/Account.cs
Models/RegisterViewModel.cs
Models/LoginViewModel.cs
Services/IAccountStore.cs
Services/AccountStore.cs
Controllers/AccountController.cs
Controllers/LoanApprovalController.cs   ← add [Authorize]
Views/Account/Register.cshtml
Views/Account/Login.cshtml
Views/Shared/_Layout.cshtml             ← auth-aware nav
Program.cs                              ← AddAuthentication, UseAuthentication
CalculatorMVC.csproj                    ← add BCrypt.Net-Next package
```

---

**Status: Not yet implemented**
