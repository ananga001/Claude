namespace CalculatorMVC.Chain;

using CalculatorMVC.Models;

public class ManagerApprover : LoanApproverBase
{
    public override void Handle(LoanApplication loan)
    {
        loan.CurrentQueue = "Manager";
    }
}
