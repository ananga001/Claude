namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public class LoanStore : ILoanStore
{
    private readonly List<LoanApplication> _loans = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public void Add(LoanApplication loan)
    {
        lock (_lock)
        {
            loan.Id = _nextId++;
            loan.SubmittedAt = DateTime.Now;
            _loans.Add(loan);
        }
    }

    public IReadOnlyList<LoanApplication> GetByQueue(string queue)
    {
        lock (_lock)
            return _loans.Where(l => l.CurrentQueue == queue && l.Status == LoanStatus.Pending).ToList();
    }

    public LoanApplication? Approve(int id, string approverRole)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return null;
            loan.Status = LoanStatus.Approved;
            loan.ApprovedBy = approverRole;
            loan.ApprovedAt = DateTime.Now;
            return loan;
        }
    }
}
