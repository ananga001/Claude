Seed the CalculatorMVC UserStore with sample Indian-named users in the correct hierarchy order.

## Steps

1. Ensure the CalculatorMVC app is running. If not, start it:
   ```
   dotnet run --project CalculatorMVC\CalculatorMVC.csproj
   ```
   Note the port from the output (typically http://localhost:5000 or https://localhost:7000).

2. Seed a **Manager** first (no Reporting To required):
   - Name: Any Indian name (e.g. Rajesh Kumar)
   - Date of Birth: e.g. 1972-04-15
   - Role: Manager
   - POST to /User/Create

3. Seed a **Supervisor** (must report to the Manager created above):
   - Name: Any Indian name (e.g. Priya Sharma)
   - Date of Birth: e.g. 1985-08-22
   - Role: Supervisor
   - ReportingToId: the Manager's Id (1)
   - POST to /User/Create

4. Seed a **Normal user** (must report to the Supervisor created above):
   - Name: Any Indian name (e.g. Amit Patel)
   - Date of Birth: e.g. 1995-03-10
   - Role: Normal
   - ReportingToId: the Supervisor's Id (2)
   - POST to /User/Create

## How to POST

Use PowerShell Invoke-WebRequest for each user, e.g.:

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/User/Create" -Method POST `
  -ContentType "application/x-www-form-urlencoded" `
  -Body "Name=Rajesh+Kumar&DateOfBirth=1972-04-15&Role=2&ReportingToId="
```

Role values: Normal=0, Supervisor=1, Manager=2

## Verify

Visit http://localhost:5000/User — all three users should appear in their respective sections with the correct reporting hierarchy shown.
