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
    private readonly ILogger<LoanApprovalController> _logger;

    public LoanApprovalController(ILoanStore store, IEmailService email, ILogger<LoanApprovalController> logger)
    {
        _store = store;
        _email = email;
        _logger = logger;
    }

    public IActionResult Index() => View(new LoanApplication());

    [HttpPost]
    public IActionResult Submit(LoanApplication model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        model.SubmittedByUsername = User.Identity!.Name!;

        var normal = new NormalApprover();
        normal.SetNext(new SupervisorApprover()).SetNext(new ManagerApprover());
        normal.Handle(model);

        _store.Add(model);
        _logger.LogInformation("Loan submitted by {User}: Applicant={Applicant}, Amount={Amount}, Queue={Queue}",
            User.Identity!.Name, model.ApplicantName, model.Amount, model.CurrentQueue);
        TempData["Message"] = $"Loan submitted successfully. Routed to {model.CurrentQueue} queue.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Normal")]
    public IActionResult LoanApproverUser()
        => View(_store.GetByQueue("Normal"));

    [HttpPost]
    [Authorize(Roles = "Normal")]
    public IActionResult UserApprove(int id)
    {
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan is null)
            _logger.LogWarning("UserApprove: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        else
            _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverUser");
    }

    [HttpPost]
    [Authorize(Roles = "Normal")]
    public IActionResult UserReject(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction("LoanApproverUser");
        }
        if (reason.Length > 500) reason = reason[..500];
        var loan = _store.Reject(id, User.Identity!.Name!, reason);
        if (loan is null)
            _logger.LogWarning("UserReject: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        return RedirectToAction("LoanApproverUser");
    }

    [Authorize(Roles = "Supervisor")]
    public IActionResult LoanApproverSupervisor()
        => View(_store.GetByQueue("Supervisor"));

    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    public IActionResult SupervisorApprove(int id)
    {
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan is null)
            _logger.LogWarning("SupervisorApprove: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        else
            _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverSupervisor");
    }

    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    public IActionResult SupervisorReject(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction("LoanApproverSupervisor");
        }
        if (reason.Length > 500) reason = reason[..500];
        var loan = _store.Reject(id, User.Identity!.Name!, reason);
        if (loan is null)
            _logger.LogWarning("SupervisorReject: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        return RedirectToAction("LoanApproverSupervisor");
    }

    [Authorize(Roles = "Manager")]
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
    [Authorize(Roles = "Manager")]
    public IActionResult ManagerApprove(int id)
    {
        var loan = _store.Approve(id, User.Identity!.Name!);
        if (loan is null)
            _logger.LogWarning("ManagerApprove: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        else
            _email.SendApprovalEmail(loan);
        return RedirectToAction("LoanApproverManager");
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public IActionResult ManagerReject(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction("LoanApproverManager");
        }
        if (reason.Length > 500) reason = reason[..500];
        var loan = _store.Reject(id, User.Identity!.Name!, reason);
        if (loan is null)
            _logger.LogWarning("ManagerReject: loan {Id} not found or not Pending, by {User}", id, User.Identity!.Name);
        return RedirectToAction("LoanApproverManager");
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public IActionResult Disburse(int id)
    {
        var loan = _store.Disburse(id, User.Identity!.Name!);
        if (loan is null)
        {
            _logger.LogWarning("Disburse: loan {Id} not found or not Approved, by {User}", id, User.Identity!.Name);
            TempData["Error"] = "Loan not found or not in Approved status.";
        }
        else
        {
            _logger.LogInformation("Loan {Id} disbursed by {User}", id, User.Identity!.Name);
        }
        return RedirectToAction("LoanApproverManager");
    }

    public IActionResult Details(int id)
    {
        var loan = _store.GetById(id);
        if (loan is null) return NotFound();

        bool isStaff = User.IsInRole("Manager") || User.IsInRole("Supervisor");
        bool isOwner = loan.SubmittedByUsername == User.Identity!.Name;
        if (!isStaff && !isOwner)
            return Forbid();

        return View(loan);
    }

    [Authorize(Roles = "Manager,Supervisor")]
    public IActionResult AllLoans()
        => View(_store.GetAll());
}
