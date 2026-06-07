namespace CalculatorMVC.Models;

public class ManagerQueueViewModel
{
    public IReadOnlyList<LoanApplication> PendingLoans { get; set; } = [];
    public IReadOnlyList<LoanApplication> ApprovedLoans { get; set; } = [];
}
