# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Structure

- **ClaudeSol001.sln** — Visual Studio 2022 solution (v17)
- **ConsoleApp1/** — .NET 9 console app. See [ConsoleApp1/README.md](./ConsoleApp1/README.md)
- **HelloWorldWinForms/** — .NET 9 WinForms app. See [HelloWorldWinForms/README.md](./HelloWorldWinForms/README.md)

Each project has its own `README.md` for project-specific details. This file covers solution-wide guidance only.

## Build & Run Commands

```powershell
# Build entire solution
dotnet build ClaudeSol001.sln

# Run console app
dotnet run --project ConsoleApp1\ConsoleApp1.csproj

# Run WinForms app
dotnet run --project HelloWorldWinForms\HelloWorldWinForms.csproj

# Build in Release mode
dotnet build ClaudeSol001.sln -c Release
```

## Project Configuration

| Setting | ConsoleApp1 | HelloWorldWinForms |
|---|---|---|
| Framework | `net9.0` | `net9.0-windows` |
| Output type | `Exe` | `WinExe` |
| Nullable | enabled | enabled |
| Implicit usings | enabled | enabled |
| WinForms | — | enabled |

## Documentation Structure

```
ClaudeSol001/
├── README.md              ← solution overview, links to projects
├── CLAUDE.md              ← this file (Claude Code guidance)
├── ConsoleApp1/
│   └── README.md          ← ConsoleApp1-specific docs
└── HelloWorldWinForms/
    └── README.md          ← HelloWorldWinForms-specific docs
```

As projects grow, add project-specific sections below rather than creating per-project `CLAUDE.md` files.

## ConsoleApp1

Currently a minimal Hello World. Entry point: `ConsoleApp1/Program.cs`.

## HelloWorldWinForms

Single form with a "Hello, World!" label. Key files:
- `Form1.Designer.cs` — layout (do not edit manually; use Visual Studio designer)
- `Form1.cs` — form logic
- `Program.cs` — `STAThread` entry point
