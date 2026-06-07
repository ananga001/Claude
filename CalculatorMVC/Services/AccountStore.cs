namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public class AccountStore : IAccountStore
{
    private readonly List<Account> _accounts = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public void Register(Account account)
    {
        lock (_lock)
        {
            account.Id = _nextId++;
            _accounts.Add(account);
        }
    }

    public Account? FindByUsername(string username)
    {
        lock (_lock)
            return _accounts.FirstOrDefault(a =>
                string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public Account? ValidatePassword(string username, string password)
    {
        var account = FindByUsername(username);
        if (account is null) return null;
        return BCrypt.Net.BCrypt.Verify(password, account.PasswordHash) ? account : null;
    }
}
