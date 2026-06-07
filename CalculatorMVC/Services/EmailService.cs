namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public void SendApprovalEmail(LoanApplication loan)
    {
        _logger.LogInformation(
            "Email sent: Loan #{Id} for {Name} (${Amount}) approved by {ApprovedBy}.",
            loan.Id, loan.ApplicantName, loan.Amount, loan.ApprovedBy);
    }
}
