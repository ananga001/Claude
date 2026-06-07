namespace CalculatorMVC.Chain;

using CalculatorMVC.Models;

public class SupervisorApprover : LoanApproverBase
{
    public override void Handle(LoanApplication loan)
    {
        if (loan.Amount < 1000)
            loan.CurrentQueue = "Supervisor";
        else
            _next?.Handle(loan);
    }
}
