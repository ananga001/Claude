using System.Security.Claims;
using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CalculatorMVC.Controllers;

public class AccountController : Controller
{
    private readonly IAccountStore _store;

    public AccountController(IAccountStore store) => _store = store;

    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
            ModelState.AddModelError("Username", "Username is required.");

        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError("Password", "Password is required.");

        if (model.Password != model.ConfirmPassword)
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");

        if (ModelState.IsValid && _store.FindByUsername(model.Username) != null)
            ModelState.AddModelError("Username", "Username is already taken.");

        if (!ModelState.IsValid)
            return View(model);

        var account = new Account
        {
            Username     = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role         = model.Role
        };
        _store.Register(account);

        await SignInAsync(account);
        return RedirectToAction("Index", "LoanApproval");
    }

    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var account = _store.ValidatePassword(model.Username, model.Password);
        if (account is null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        await SignInAsync(account);
        return RedirectToAction("Index", "LoanApproval");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
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
