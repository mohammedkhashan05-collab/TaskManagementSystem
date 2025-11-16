using Serilog;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> CreateUserAsync(User user, string password)
    {
        Log.Information("Creating new user: {Username}, Email: {Email}, Role: {Role}", 
            user.Username, user.Email, user.Role);

        // Check if username or email already exists
        var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (existingUser != null)
        {
            Log.Warning("User creation failed: Username {Username} already exists", user.Username);
            throw new InvalidOperationException("Username already exists");
        }

        existingUser = await _userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
        {
            Log.Warning("User creation failed: Email {Email} already exists", user.Email);
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;

        var createdUser = await _userRepository.AddAsync(user);
        Log.Information("User created successfully. UserId: {UserId}, Username: {Username}", 
            createdUser.Id, createdUser.Username);
        
        return createdUser;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
            throw new InvalidOperationException("User not found");

        // Check if username or email is being changed and if it conflicts
        if (existingUser.Username != user.Username)
        {
            var usernameExists = await _userRepository.GetByUsernameAsync(user.Username);
            if (usernameExists != null)
                throw new InvalidOperationException("Username already exists");
        }

        if (existingUser.Email != user.Email)
        {
            var emailExists = await _userRepository.GetByEmailAsync(user.Email);
            if (emailExists != null)
                throw new InvalidOperationException("Email already exists");
        }

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        Log.Debug("Authenticating user: {Username}", username);
        
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            Log.Warning("Authentication failed: User {Username} not found", username);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            Log.Warning("Authentication failed: Invalid password for user {Username}", username);
            return null;
        }

        Log.Information("User {Username} authenticated successfully", username);
        return user;
    }
}


