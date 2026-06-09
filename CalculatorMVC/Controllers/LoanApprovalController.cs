using CalculatorMVC.Chain;
using CalculatorMVC.Models;
using CalculatorMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CalculatorMVC.Chain.LoanQueue;

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

    private const int MaxRejectionReasonLength = 500;

    [Authorize(Roles = "Normal")]
    public IActionResult LoanApproverUser()
        => View(_store.GetByQueue(Normal));

    [HttpPost, Authorize(Roles = "Normal")]
    public IActionResult UserApprove(int id)       => ApproveCore(id, "LoanApproverUser");

    [HttpPost, Authorize(Roles = "Normal")]
    public IActionResult UserReject(int id, string reason) => RejectCore(id, reason, "LoanApproverUser");

    [Authorize(Roles = "Supervisor")]
    public IActionResult LoanApproverSupervisor()
        => View(_store.GetByQueue(Supervisor));

    [HttpPost, Authorize(Roles = "Supervisor")]
    public IActionResult SupervisorApprove(int id) => ApproveCore(id, "LoanApproverSupervisor");

    [HttpPost, Authorize(Roles = "Supervisor")]
    public IActionResult SupervisorReject(int id, string reason) => RejectCore(id, reason, "LoanApproverSupervisor");

    [Authorize(Roles = "Manager")]
    public IActionResult LoanApproverManager()
    {
        var vm = new ManagerQueueViewModel
        {
            PendingLoans  = _store.GetByQueue(Manager),
            ApprovedLoans = _store.GetApprovedByQueue(Manager)
        };
        return View(vm);
    }

    [HttpPost, Authorize(Roles = "Manager")]
    public IActionResult ManagerApprove(int id)           => ApproveCore(id, "LoanApproverManager");

    [HttpPost, Authorize(Roles = "Manager")]
    public IActionResult ManagerReject(int id, string reason) => RejectCore(id, reason, "LoanApproverManager");

    private IActionResult ApproveCore(int id, string redirectAction)
    {
        var approver = User.Identity!.Name!;
        var existing = _store.GetById(id);
        if (existing?.SubmittedByUsername == approver)
        {
            TempData["Error"] = "You cannot approve your own loan application.";
            return RedirectToAction(redirectAction);
        }
        var loan = _store.Approve(id, approver);
        if (loan is null)
            _logger.LogWarning("Approve: loan {Id} not found or not Pending, by {User}", id, approver);
        else
            _email.SendApprovalEmail(loan);
        return RedirectToAction(redirectAction);
    }

    private IActionResult RejectCore(int id, string reason, string redirectAction)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction(redirectAction);
        }
        if (reason.Length > MaxRejectionReasonLength)
            reason = reason[..MaxRejectionReasonLength];
        var rejector = User.Identity!.Name!;
        var loan = _store.Reject(id, rejector, reason);
        if (loan is null)
            _logger.LogWarning("Reject: loan {Id} not found or not Pending, by {User}", id, rejector);
        return RedirectToAction(redirectAction);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public IActionResult Disburse(int id)
    {
        var disburser = User.Identity!.Name!;
        var existing  = _store.GetById(id);

        if (existing is null || existing.Status != LoanStatus.Approved)
        {
            TempData["Error"] = "Loan not found or not in Approved status.";
            return RedirectToAction("LoanApproverManager");
        }
        if (existing.ApprovedBy == disburser)
        {
            TempData["Error"] = "The manager who approved this loan cannot also disburse it.";
            return RedirectToAction("LoanApproverManager");
        }

        var loan = _store.Disburse(id, disburser);
        if (loan is null)
            _logger.LogWarning("Disburse: loan {Id} not found or not Approved, by {User}", id, disburser);
        else
            _logger.LogInformation("Loan {Id} disbursed by {User}", id, disburser);

        return RedirectToAction("LoanApproverManager");
    }

    public IActionResult Details(int id)
    {
        var loan = _store.GetById(id);
        if (loan is null) return NotFound();

        bool isStaff = User.IsInRole("Manager") || User.IsInRole("Supervisor");
        bool isOwner = loan.SubmittedByUsername == User.Identity!.Name;
        if (!isStaff && !isOwner)
        {
            _logger.LogWarning("Unauthorized Details access: loan {Id} by {User}", id, User.Identity!.Name);
            return Forbid();
        }

        return View(loan);
    }

    [Authorize(Roles = "Manager,Supervisor")]
    public IActionResult AllLoans()
        => View(_store.GetAll().OrderByDescending(l => l.SubmittedAt).ToList());
}
