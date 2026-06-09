using System.ComponentModel.DataAnnotations;
using CalculatorMVC.Chain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CalculatorMVC.Models;

public enum LoanStatus { Pending, Approved, Rejected, Disbursed }

public class LoanApplication
{
    [BindNever] public int Id { get; set; }
    [BindNever] public string CurrentQueue { get; set; } = LoanQueue.Normal;
    [BindNever] public LoanStatus Status { get; set; } = LoanStatus.Pending;
    [BindNever] public string? SubmittedByUsername { get; set; }
    [BindNever] public string? ApprovedBy { get; set; }
    [BindNever] public DateTime SubmittedAt { get; set; }
    [BindNever] public DateTime? ApprovedAt { get; set; }
    [BindNever] public string? RejectionReason { get; set; }
    [BindNever] public string? RejectedBy { get; set; }
    [BindNever] public DateTime? RejectedAt { get; set; }
    [BindNever] public string? DisbursedBy { get; set; }
    [BindNever] public DateTime? DisbursedAt { get; set; }
    [BindNever] public List<LoanRepayment> Repayments { get; set; } = new();

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = "";

    [MaxLength(500)]
    public string Purpose { get; set; } = "";

    [Range(0.01, 10_000_000, ErrorMessage = "Amount must be between $0.01 and $10,000,000.")]
    public decimal Amount { get; set; }
}
