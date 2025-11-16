using TaskManagement.Core.Entities;

namespace TaskManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any())
            return;

        // Seed Users
        var admin = new User
        {
            Username = "admin",
            Email = "admin@taskmanagement.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        var regularUser = new User
        {
            Username = "user",
            Email = "user@taskmanagement.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, regularUser);
        context.SaveChanges();

        // Seed Tasks
        var tasks = new List<Core.Entities.Task>
        {
            new Core.Entities.Task
            {
                Title = "Implement Authentication",
                Description = "Implement JWT-based authentication for the API",
                Status = "Completed",
                AssignedUserId = admin.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Core.Entities.Task
            {
                Title = "Create User Dashboard",
                Description = "Build a responsive dashboard for users to view their tasks",
                Status = "InProgress",
                AssignedUserId = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Core.Entities.Task
            {
                Title = "Add Task Filtering",
                Description = "Implement filtering and sorting functionality for tasks",
                Status = "Pending",
                AssignedUserId = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}


