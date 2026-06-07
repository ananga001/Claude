namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface IUserStore
{
    void Add(User user);
    IReadOnlyList<User> GetAll();
    IReadOnlyList<User> GetByRole(UserRole role);
    User? GetById(int id);
}
