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
            loan.SubmittedAt = DateTime.UtcNow;
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
            var loan = _loans.FirstOrDefault(l => l.Id == id && l.Status == LoanStatus.Pending);
            if (loan is null) return null;
            loan.Status = LoanStatus.Approved;
            loan.ApprovedBy = approvedBy;
            loan.ApprovedAt = DateTime.UtcNow;
            GenerateRepaymentSchedule(loan);
            return loan;
        }
    }

    public LoanApplication? Reject(int id, string rejectedBy, string reason)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == id && l.Status == LoanStatus.Pending);
            if (loan is null) return null;
            loan.Status = LoanStatus.Rejected;
            loan.RejectedBy = rejectedBy;
            loan.RejectionReason = reason;
            loan.RejectedAt = DateTime.UtcNow;
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
            loan.DisbursedAt = DateTime.UtcNow;
            return loan;
        }
    }

    public IReadOnlyList<LoanApplication> GetAll()
    {
        lock (_lock) return _loans.ToList();
    }

    public IReadOnlyList<LoanApplication> GetApprovedByQueue(string queue)
    {
        lock (_lock)
            return _loans
                .Where(l => l.CurrentQueue == queue && l.Status == LoanStatus.Approved)
                .ToList();
    }

    public LoanApplication? GetById(int id)
    {
        lock (_lock) return _loans.FirstOrDefault(l => l.Id == id);
    }

    // EMI amortization — defaults: 12 months, 10% annual (form doesn't collect term/rate)
    private static void GenerateRepaymentSchedule(LoanApplication loan)
    {
        const int     termMonths        = 12;
        const decimal annualRatePercent = 10.0m;

        decimal monthlyRate = annualRatePercent / 100m / 12m;
        decimal principal   = loan.Amount;
        decimal factor      = DecimalPow(1m + monthlyRate, termMonths);
        decimal emi         = principal * monthlyRate * factor / (factor - 1m);

        decimal balance   = principal;
        var     startDate = loan.ApprovedAt ?? DateTime.UtcNow;

        for (int i = 1; i <= termMonths; i++)
        {
            decimal interest      = Math.Round(balance * monthlyRate, 2);
            decimal principalPart = Math.Round(emi - interest, 2);
            balance = Math.Round(balance - principalPart, 2);

            // Absorb any residual rounding cent on the final installment
            if (i == termMonths) balance = 0m;

            loan.Repayments.Add(new LoanRepayment
            {
                Id                = i,
                LoanId            = loan.Id,
                InstallmentNumber = i,
                DueDate           = startDate.AddMonths(i),
                Principal         = principalPart,
                Interest          = interest,
                TotalDue          = Math.Round(emi, 2),
                Balance           = Math.Max(balance, 0m)
            });
        }
    }

    private static decimal DecimalPow(decimal baseVal, int exp)
    {
        decimal result = 1m;
        for (int i = 0; i < exp; i++) result *= baseVal;
        return result;
    }
}
