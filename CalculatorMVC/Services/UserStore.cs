namespace CalculatorMVC.Services;

using CalculatorMVC.Models;

public class UserStore : IUserStore
{
    private readonly List<User> _users = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public void Add(User user)
    {
        lock (_lock)
        {
            user.Id = _nextId++;
            _users.Add(user);
        }
    }

    public IReadOnlyList<User> GetAll()
    {
        lock (_lock) return _users.ToList();
    }

    public IReadOnlyList<User> GetByRole(UserRole role)
    {
        lock (_lock) return _users.Where(u => u.Role == role).ToList();
    }

    public User? GetById(int id)
    {
        lock (_lock) return _users.FirstOrDefault(u => u.Id == id);
    }
}
