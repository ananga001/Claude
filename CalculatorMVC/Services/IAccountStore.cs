namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public interface IAccountStore
{
    void Register(Account account);
    Account? FindByUsername(string username);
    Account? ValidatePassword(string username, string password);
    bool IsLockedOut(string username);
}
