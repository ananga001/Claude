using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CalculatorMVC.Controllers;

[Authorize(Roles = "Manager")]
public class UserController : Controller
{
    private readonly IUserStore _store;

    public UserController(IUserStore store) => _store = store;

    public IActionResult Index()
    {
        var all = _store.GetAll();
        var vm = new UserIndexViewModel
        {
            Managers    = all.Where(u => u.Role == UserRole.Manager).ToList(),
            Supervisors = all.Where(u => u.Role == UserRole.Supervisor).ToList(),
            NormalUsers = all.Where(u => u.Role == UserRole.Normal).ToList(),
            ReportingToNames = all
                .Where(u => u.ReportingToId.HasValue)
                .ToDictionary(
                    u => u.Id,
                    u => _store.GetById(u.ReportingToId!.Value)?.Name ?? "Unknown")
        };
        return View(vm);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View(new User());
    }

    [HttpPost]
    public IActionResult Create(User model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("Name", "Name is required.");

        if (model.Role != UserRole.Manager && model.ReportingToId is null)
            ModelState.AddModelError("ReportingToId",
                model.Role == UserRole.Supervisor
                    ? "A Supervisor must report to a Manager."
                    : "A Normal user must report to a Supervisor.");

        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return View(model);
        }

        _store.Add(model);
        TempData["Message"] = $"User '{model.Name}' added as {model.Role}.";
        return RedirectToAction("Index");
    }

    private void PopulateDropdowns()
    {
        ViewBag.Managers    = _store.GetByRole(UserRole.Manager);
        ViewBag.Supervisors = _store.GetByRole(UserRole.Supervisor);
    }
}
