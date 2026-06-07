# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Structure

- **ClaudeSol001.sln** — Visual Studio 2022 solution (v17)
- **ConsoleApp1/** — .NET 9 console app. See [ConsoleApp1/README.md](./ConsoleApp1/README.md)
- **HelloWorldWinForms/** — .NET 9 WinForms app. See [HelloWorldWinForms/README.md](./HelloWorldWinForms/README.md)
- **CalculatorMVC/** — .NET 9 ASP.NET Core MVC web app with Add/Subtract/Multiply/Divide calculator

Each project has its own `README.md` for project-specific details. This file covers solution-wide guidance only.

## Build & Run Commands

```powershell
# Build entire solution
dotnet build ClaudeSol001.sln

# Run console app
dotnet run --project ConsoleApp1\ConsoleApp1.csproj

# Run WinForms app
dotnet run --project HelloWorldWinForms\HelloWorldWinForms.csproj

# Run Calculator MVC web app
dotnet run --project CalculatorMVC\CalculatorMVC.csproj

# Build in Release mode
dotnet build ClaudeSol001.sln -c Release
```

## Project Configuration

| Setting | ConsoleApp1 | HelloWorldWinForms | CalculatorMVC |
|---|---|---|---|
| Framework | `net9.0` | `net9.0-windows` | `net9.0` |
| Output type | `Exe` | `WinExe` | Web |
| Nullable | enabled | enabled | enabled |
| Implicit usings | enabled | enabled | enabled |
| WinForms | — | enabled | — |

## Documentation Structure

```
ClaudeSol001/
├── README.md              ← solution overview, links to projects
├── CLAUDE.md              ← this file (Claude Code guidance)
├── ConsoleApp1/
│   └── README.md          ← ConsoleApp1-specific docs
├── HelloWorldWinForms/
│   └── README.md          ← HelloWorldWinForms-specific docs
└── CalculatorMVC/         ← ASP.NET Core MVC calculator app
```

As projects grow, add project-specific sections below rather than creating per-project `CLAUDE.md` files.

## Claude Code CLI

Useful flags when working in this repo:

| Flag | Description |
|---|---|
| `claude -r` | Resume a past session (interactive picker) |
| `claude -r <name>` | Resume a specific named session |
| `claude -c` | Continue the most recent conversation |
| `claude -n <name>` | Name the current session for later resuming |
| `claude --model <id>` | Set the model for the session |
| `claude --effort <level>` | Set effort level: `low / medium / high / max` |
| `claude /context` | Show current context window usage and token breakdown |

## ConsoleApp1

Currently a minimal Hello World. Entry point: `ConsoleApp1/Program.cs`.

## HelloWorldWinForms

Single form with a "Hello, World!" label. Key files:
- `Form1.Designer.cs` — layout (do not edit manually; use Visual Studio designer)
- `Form1.cs` — form logic
- `Program.cs` — `STAThread` entry point

## CalculatorMVC

ASP.NET Core MVC web app. Default route lands on the Calculator. Key files:

**Calculator**
- `Controllers/CalculatorController.cs` — GET index, POST calculate (add/subtract/multiply/divide)
- `Models/CalculatorModel.cs` — input + result model
- `Views/Calculator/Index.cshtml` — Bootstrap form UI with result display
- Division includes divide-by-zero guard (returns error message instead of throwing)

**Loan Approval — Chain of Responsibility**
- `Controllers/LoanApprovalController.cs` — 8 routes: submit + 3 role queues + 3 approvals; protected with `[Authorize]`
- `Models/LoanApplication.cs` — loan model with `LoanStatus` enum
- `Chain/` — `LoanApproverBase`, `NormalApprover` (<$100), `SupervisorApprover` ($100–$999), `ManagerApprover` (≥$1000)
- `Services/ILoanStore` + `LoanStore` — in-memory singleton store
- `Services/IEmailService` + `EmailService` — simulated approval email (logs to ILogger)

**User Management**
- `Controllers/UserController.cs` — Index (hierarchy view) + Create
- `Models/User.cs` — `UserRole` enum (Normal/Supervisor/Manager) + `User` with `ReportingToId`
- `Models/UserIndexViewModel.cs` — groups users by role for the Index view
- `Services/IUserStore` + `UserStore` — in-memory singleton

**Authentication (Cookie-based)**
- `Controllers/AccountController.cs` — Register, Login, Logout
- `Models/Account.cs` — username + BCrypt password hash + role
- `Models/RegisterViewModel.cs`, `LoginViewModel.cs`
- `Services/IAccountStore` + `AccountStore` — in-memory singleton; uses `BCrypt.Net-Next`
- `Views/Account/Register.cshtml`, `Login.cshtml` — Bootstrap forms
- `Program.cs` — `AddAuthentication(Cookie)`, `UseAuthentication`, `UseAuthorization`
- Unauthenticated users redirected to `/Account/Login`

**Slash Commands** (`.claude/commands/`)
- `/SeedUser` — seeds Manager → Supervisor → Normal user with Indian names via HTTP POST
