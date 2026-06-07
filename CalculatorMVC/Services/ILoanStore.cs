namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface ILoanStore
{
    void Add(LoanApplication loan);
    IReadOnlyList<LoanApplication> GetByQueue(string queue);
    LoanApplication? Approve(int id, string approvedBy);
    LoanApplication? Reject(int id, string rejectedBy, string reason);
    LoanApplication? Disburse(int id, string disbursedBy);
    IReadOnlyList<LoanApplication> GetAll();
    LoanApplication? GetById(int id);
}
