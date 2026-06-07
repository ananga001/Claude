namespace CalculatorMVC.Models;

public enum LoanStatus { Pending, Approved, Rejected, Disbursed }

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
    public string? RejectionReason { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? DisbursedBy { get; set; }
    public DateTime? DisbursedAt { get; set; }
    public List<LoanRepayment> Repayments { get; set; } = new();
}
