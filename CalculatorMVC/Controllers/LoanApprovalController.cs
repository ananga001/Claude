using CalculatorMVC.Chain;
using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CalculatorMVC.Controllers;

[Authorize]
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
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverUser");
    }

    [HttpPost]
    public IActionResult UserReject(int id, string reason)
    {
        _store.Reject(id, User.Identity!.Name!, reason);
        return RedirectToAction("LoanApproverUser");
    }

    public IActionResult LoanApproverSupervisor()
        => View(_store.GetByQueue("Supervisor"));

    [HttpPost]
    public IActionResult SupervisorApprove(int id)
    {
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverSupervisor");
    }

    [HttpPost]
    public IActionResult SupervisorReject(int id, string reason)
    {
        _store.Reject(id, User.Identity!.Name!, reason);
        return RedirectToAction("LoanApproverSupervisor");
    }

    public IActionResult LoanApproverManager()
    {
        var vm = new ManagerQueueViewModel
        {
            PendingLoans = _store.GetByQueue("Manager"),
            ApprovedLoans = _store.GetAll()
                                  .Where(l => l.CurrentQueue == "Manager"
                                           && l.Status == LoanStatus.Approved)
                                  .ToList()
        };
        return View(vm);
    }

    [HttpPost]
    public IActionResult ManagerApprove(int id)
    {
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan != null) _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverManager");
    }

    [HttpPost]
    public IActionResult ManagerReject(int id, string reason)
    {
        _store.Reject(id, User.Identity!.Name!, reason);
        return RedirectToAction("LoanApproverManager");
    }

    [HttpPost]
    public IActionResult Disburse(int id)
    {
        _store.Disburse(id, User.Identity!.Name!);
        return RedirectToAction("LoanApproverManager");
    }

    public IActionResult Details(int id)
    {
        var loan = _store.GetById(id);
        if (loan is null) return NotFound();
        return View(loan);
    }

    public IActionResult AllLoans()
        => View(_store.GetAll());
}
