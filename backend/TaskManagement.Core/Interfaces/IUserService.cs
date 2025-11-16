using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User user, string password);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<User?> AuthenticateAsync(string username, string password);
}


