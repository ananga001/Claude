using System.ComponentModel.DataAnnotations;

namespace CalculatorMVC.Models;

public class LoginViewModel
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "";

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
