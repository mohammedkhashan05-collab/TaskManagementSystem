using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests.Services;

public class UserServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateUserAsync_ValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var context = GetDbContext();
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };

        // Act
        var result = await userService.CreateUserAsync(user, "Password123!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.NotEmpty(result.PasswordHash);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateUserAsync_DuplicateUsername_ThrowsException()
    {
        // Arrange
        var context = GetDbContext();
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);

        var user1 = new User
        {
            Username = "testuser",
            Email = "test1@example.com",
            Role = "User"
        };

        var user2 = new User
        {
            Username = "testuser",
            Email = "test2@example.com",
            Role = "User"
        };

        await userService.CreateUserAsync(user1, "Password123!");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.CreateUserAsync(user2, "Password123!"));
    }

    [Fact]
    public async System.Threading.Tasks.Task AuthenticateAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var context = GetDbContext();
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };

        await userService.CreateUserAsync(user, "Password123!");

        // Act
        var result = await userService.AuthenticateAsync("testuser", "Password123!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async System.Threading.Tasks.Task AuthenticateAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var context = GetDbContext();
        var userRepository = new UserRepository(context);
        var userService = new UserService(userRepository);

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };

        await userService.CreateUserAsync(user, "Password123!");

        // Act
        var result = await userService.AuthenticateAsync("testuser", "WrongPassword");

        // Assert
        Assert.Null(result);
    }
}


