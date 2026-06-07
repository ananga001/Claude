namespace CalculatorMVC.Models;

public enum LoanStatus { Pending, Approved }

public class LoanApplication
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = "";
    public string Purpose { get; set; } = "";
    public decimal Amount { get; set; }
    public string CurrentQueue { get; set; } = "Normal";
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
