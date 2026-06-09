using System.Security.Claims;
using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CalculatorMVC.Controllers;

public class AccountController : Controller
{
    private readonly IAccountStore _store;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountStore store, ILogger<AccountController> logger)
    {
        _store = store;
        _logger = logger;
    }

    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_store.FindByUsername(model.Username) != null)
        {
            ModelState.AddModelError("Username", "Username is already taken.");
            return View(model);
        }

        var account = new Account
        {
            Username     = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role         = UserRole.Normal
        };
        _store.Register(account);
        _logger.LogInformation("New account registered: {Username}", account.Username);

        await SignInAsync(account);
        return RedirectToAction("Index", "LoanApproval");
    }

    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var account = _store.ValidatePassword(model.Username, model.Password);
        if (account is null)
        {
            _logger.LogWarning("Failed login for {Username} from {IP}",
                model.Username, HttpContext.Connection.RemoteIpAddress);
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        _logger.LogInformation("User {Username} logged in from {IP}",
            account.Username, HttpContext.Connection.RemoteIpAddress);
        await SignInAsync(account);
        return RedirectToAction("Index", "LoanApproval");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {Username} logged out", User.Identity?.Name);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInAsync(Account account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, account.Username),
            new(ClaimTypes.Role, account.Role.ToString())
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
