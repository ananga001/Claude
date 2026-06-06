# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Structure

- **ClaudeSol001.sln** — Visual Studio 17 solution file
- **ConsoleApp1/** — Single .NET 9 console application project (`ConsoleApp1.csproj`)

## Build & Run Commands

```powershell
# Build the solution
dotnet build ClaudeSol001.sln

# Run the console app
dotnet run --project ConsoleApp1\ConsoleApp1.csproj

# Build in Release mode
dotnet build ClaudeSol001.sln -c Release
```

## Project Configuration

- Target framework: `net9.0`
- Nullable reference types: enabled
- Implicit usings: enabled
- Entry point: `ConsoleApp1\Program.cs` (top-level statements)
