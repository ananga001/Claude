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

    public LoanApplication? Approve(int id, string approvedBy)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return null;
            loan.Status = LoanStatus.Approved;
            loan.ApprovedBy = approvedBy;
            loan.ApprovedAt = DateTime.Now;
            GenerateRepaymentSchedule(loan);
            return loan;
        }
    }

    public LoanApplication? Reject(int id, string rejectedBy, string reason)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id);
            if (loan is null) return null;
            loan.Status = LoanStatus.Rejected;
            loan.RejectedBy = rejectedBy;
            loan.RejectionReason = reason;
            loan.RejectedAt = DateTime.Now;
            return loan;
        }
    }

    public LoanApplication? Disburse(int id, string disbursedBy)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id && l.Status == LoanStatus.Approved);
            if (loan is null) return null;
            loan.Status = LoanStatus.Disbursed;
            loan.DisbursedBy = disbursedBy;
            loan.DisbursedAt = DateTime.Now;
            return loan;
        }
    }

    public IReadOnlyList<LoanApplication> GetAll()
    {
        lock (_lock) return _loans.ToList();
    }

    public LoanApplication? GetById(int id)
    {
        lock (_lock) return _loans.FirstOrDefault(l => l.Id == id);
    }

    // EMI amortization — defaults: 12 months, 10% annual (form doesn't collect term/rate)
    private static void GenerateRepaymentSchedule(LoanApplication loan)
    {
        const int termMonths = 12;
        const double annualRatePercent = 10.0;

        double monthlyRate = annualRatePercent / 100.0 / 12.0;
        double principal = (double)loan.Amount;
        double emi = principal * monthlyRate * Math.Pow(1 + monthlyRate, termMonths)
                     / (Math.Pow(1 + monthlyRate, termMonths) - 1);

        double balance = principal;
        var startDate = loan.ApprovedAt ?? DateTime.Now;

        for (int i = 1; i <= termMonths; i++)
        {
            double interest = balance * monthlyRate;
            double principalPart = emi - interest;
            balance -= principalPart;

            loan.Repayments.Add(new LoanRepayment
            {
                Id = i,
                LoanId = loan.Id,
                InstallmentNumber = i,
                DueDate = startDate.AddMonths(i),
                Principal = (decimal)Math.Round(principalPart, 2),
                Interest = (decimal)Math.Round(interest, 2),
                TotalDue = (decimal)Math.Round(emi, 2),
                Balance = (decimal)Math.Round(Math.Max(balance, 0), 2)
            });
        }
    }
}
