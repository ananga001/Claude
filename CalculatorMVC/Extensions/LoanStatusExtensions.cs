namespace CalculatorMVC.Extensions;

using CalculatorMVC.Models;

public static class LoanStatusExtensions
{
    public static string BadgeColor(this LoanStatus status) => status switch
    {
        LoanStatus.Pending   => "secondary",
        LoanStatus.Approved  => "success",
        LoanStatus.Rejected  => "danger",
        LoanStatus.Disbursed => "primary",
        _                    => "secondary"
    };
}
