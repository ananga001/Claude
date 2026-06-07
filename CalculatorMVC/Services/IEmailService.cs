namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface IEmailService
{
    void SendApprovalEmail(LoanApplication loan);
}
