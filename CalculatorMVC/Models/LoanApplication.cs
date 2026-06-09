using System.ComponentModel.DataAnnotations;

namespace CalculatorMVC.Models;

public enum LoanStatus { Pending, Approved, Rejected, Disbursed }

public class LoanApplication
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = "";

    [MaxLength(500)]
    public string Purpose { get; set; } = "";

    [Range(0.01, 10_000_000, ErrorMessage = "Amount must be between $0.01 and $10,000,000.")]
    public decimal Amount { get; set; }

    public string CurrentQueue { get; set; } = "Normal";
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public string? SubmittedByUsername { get; set; }
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
