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
        lock (_lock)
        {
            var account = _accounts.FirstOrDefault(a =>
                string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            if (account is null) return null;

            if (account.LockedUntil.HasValue && account.LockedUntil > DateTime.UtcNow)
                return null;

            if (BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
            {
                account.FailedLoginAttempts = 0;
                account.LockedUntil = null;
                return account;
            }

            account.FailedLoginAttempts++;
            if (account.FailedLoginAttempts >= 5)
                account.LockedUntil = DateTime.UtcNow.AddMinutes(15);

            return null;
        }
    }

    public bool IsLockedOut(string username)
    {
        lock (_lock)
        {
            var account = _accounts.FirstOrDefault(a =>
                string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            return account?.LockedUntil.HasValue == true && account.LockedUntil > DateTime.UtcNow;
        }
    }
}
