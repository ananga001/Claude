namespace CalculatorMVC.Chain;

using CalculatorMVC.Models;

public abstract class LoanApproverBase
{
    protected LoanApproverBase? _next;

    public LoanApproverBase SetNext(LoanApproverBase next)
    {
        _next = next;
        return next;
    }

    public abstract void Handle(LoanApplication loan);
}
