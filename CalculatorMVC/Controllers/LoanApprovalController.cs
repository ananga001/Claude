using CalculatorMVC.Chain;
using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalculatorMVC.Controllers;

public class LoanApprovalController : Controller
{
    private readonly ILoanStore _store;
    private readonly IEmailService _email;

    public LoanApprovalController(ILoanStore store, IEmailService email)
    {
        _store = store;
        _email = email;
    }

    public IActionResult Index() => View(new LoanApplication());

    [HttpPost]
    public IActionResult Submit(LoanApplication model)
    {
        if (string.IsNullOrWhiteSpace(model.ApplicantName) || model.Amount <= 0)
        {
            ModelState.AddModelError("", "Applicant name and a positive amount are required.");
            return View("Index", model);
        }

        var normal = new NormalApprover();
        normal.SetNext(new SupervisorApprover()).SetNext(new ManagerApprover());
        normal.Handle(model);

        _store.Add(model);
        TempData["Message"] = $"Loan submitted successfully. Routed to {model.CurrentQueue} queue.";
        return RedirectToAction("Index");
    }

    public IActionResult LoanApproverUser()
        => View(_store.GetByQueue("Normal"));

    [HttpPost]
    public IActionResult UserApprove(int id)
    {
        var loan = _store.Approve(id, "Normal User");
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverUser");
    }

    public IActionResult LoanApproverSupervisor()
        => View(_store.GetByQueue("Supervisor"));

    [HttpPost]
    public IActionResult SupervisorApprove(int id)
    {
        var loan = _store.Approve(id, "Supervisor");
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverSupervisor");
    }

    public IActionResult LoanApproverManager()
        => View(_store.GetByQueue("Manager"));

    [HttpPost]
    public IActionResult ManagerApprove(int id)
    {
        var loan = _store.Approve(id, "Manager");
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverManager");
    }
}
