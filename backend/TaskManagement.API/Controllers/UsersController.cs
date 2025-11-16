using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets all users. Only admins can access this endpoint.
    /// </summary>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an admin</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }));
    }

    /// <summary>
    /// Gets a specific user by ID. Users can view their own profile, admins can view any user.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <returns>The user details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user tries to access another user's profile without admin rights</response>
    /// <response code="404">If user is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Creates a new user. Only admins can create users.
    /// </summary>
    /// <param name="request">User creation request containing username, email, password, and role</param>
    /// <returns>The created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the request is invalid or required fields are missing</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an admin</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Log.Information("Admin {AdminId} attempting to create user: {Username}", adminId, request.Username);

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            Log.Warning("User creation failed: Missing required fields");
            return BadRequest(new { message = "Username, email, and password are required" });
        }

        try
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role ?? "User"
            };

            var createdUser = await _userService.CreateUserAsync(user, request.Password);
            Log.Information("User created successfully. UserId: {UserId}, Username: {Username}, Role: {Role}", 
                createdUser.Id, createdUser.Username, createdUser.Role);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new UserDto
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                Role = createdUser.Role,
                CreatedAt = createdUser.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing user. Users can update their own profile (except role), admins can update any user.
    /// </summary>
    /// <param name="id">The ID of the user to update</param>
    /// <param name="request">User update request containing fields to update</param>
    /// <returns>The updated user</returns>
    /// <response code="200">Returns the updated user</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user tries to update another user's profile without admin rights</response>
    /// <response code="404">If user is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        try
        {
            user.Username = request.Username ?? user.Username;
            user.Email = request.Email ?? user.Email;
            user.Role = request.Role ?? user.Role;

            var updatedUser = await _userService.UpdateUserAsync(user);
            return Ok(new UserDto
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                Role = updatedUser.Role,
                CreatedAt = updatedUser.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Error updating user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a user by ID. Only admins can delete users.
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an admin</response>
    /// <response code="404">If user is not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Log.Information("Admin {AdminId} attempting to delete user {UserId}", adminId, id);

        var result = await _userService.DeleteUserAsync(id);
        if (!result)
        {
            Log.Warning("User {UserId} not found for deletion", id);
            return NotFound(new { message = "User not found" });
        }

        Log.Information("User {UserId} deleted successfully by admin {AdminId}", id, adminId);
        return NoContent();
    }
}

/// <summary>
/// User data transfer object
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier of the user
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Username of the user
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the user
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Role of the user (Admin or User)
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Username for the new user (required)
    /// </summary>
    /// <example>newuser</example>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address for the new user (required)
    /// </summary>
    /// <example>newuser@example.com</example>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for the new user (required)
    /// </summary>
    /// <example>Password123!</example>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Role for the new user (defaults to "User" if not provided)
    /// </summary>
    /// <example>User</example>
    public string? Role { get; set; }
}

/// <summary>
/// Request model for updating an existing user
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// New username for the user
    /// </summary>
    /// <example>updatedusername</example>
    public string? Username { get; set; }
    
    /// <summary>
    /// New email address for the user
    /// </summary>
    /// <example>updatedemail@example.com</example>
    public string? Email { get; set; }
    
    /// <summary>
    /// New role for the user (only admins can change roles)
    /// </summary>
    /// <example>Admin</example>
    public string? Role { get; set; }
}


