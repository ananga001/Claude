namespace CalculatorMVC.Chain;

using CalculatorMVC.Models;

public class NormalApprover : LoanApproverBase
{
    public override void Handle(LoanApplication loan)
    {
        if (loan.Amount < 100)
            loan.CurrentQueue = "Normal";
        else
            _next?.Handle(loan);
    }
}
