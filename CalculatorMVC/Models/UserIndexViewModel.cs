namespace CalculatorMVC.Models;

public class UserIndexViewModel
{
    public IReadOnlyList<User> Managers { get; init; } = [];
    public IReadOnlyList<User> Supervisors { get; init; } = [];
    public IReadOnlyList<User> NormalUsers { get; init; } = [];
    public Dictionary<int, string> ReportingToNames { get; init; } = [];
}
