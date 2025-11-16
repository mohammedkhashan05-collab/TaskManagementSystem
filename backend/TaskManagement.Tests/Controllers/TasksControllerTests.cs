using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.API;
using TaskManagement.Infrastructure.Data;
using Xunit;

namespace TaskManagement.Tests.Controllers;

public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TasksControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("token").GetString() ?? string.Empty;
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasks_AsAdmin_ReturnsAllTasks()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_AsAdmin_ReturnsCreatedTask()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createTaskRequest = new
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = "Pending",
            AssignedUserId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createTaskRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Test Task", task.GetProperty("title").GetString());
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createTaskRequest = new
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = "Pending",
            AssignedUserId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createTaskRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}


