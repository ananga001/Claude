namespace CalculatorMVC.Models;

public enum UserRole { Normal, Supervisor, Manager }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public UserRole Role { get; set; }
    public int? ReportingToId { get; set; }
}
