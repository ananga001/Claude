using System.ComponentModel.DataAnnotations;

namespace CalculatorMVC.Models;

public class RegisterViewModel
{
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$",
        ErrorMessage = "Username may only contain letters, numbers, underscores, and hyphens.")]
    public string Username { get; set; } = "";

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [MaxLength(100)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, a digit, and a special character.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
