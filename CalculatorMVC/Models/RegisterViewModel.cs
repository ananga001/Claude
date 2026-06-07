namespace CalculatorMVC.Models;

public class RegisterViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public UserRole Role { get; set; }
}
