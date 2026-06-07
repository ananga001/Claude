namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface ILoanStore
{
    void Add(LoanApplication loan);
    IReadOnlyList<LoanApplication> GetByQueue(string queue);
    LoanApplication? Approve(int id, string approverRole);
}
