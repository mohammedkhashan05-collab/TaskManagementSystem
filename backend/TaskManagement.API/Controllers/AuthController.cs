using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login credentials containing username and password</param>
    /// <returns>JWT token and user information if authentication succeeds</returns>
    /// <response code="200">Returns the JWT token and user details</response>
    /// <response code="400">If username or password is missing</response>
    /// <response code="401">If username or password is invalid</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Log.Information("Login attempt for username: {Username}", request.Username);

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            Log.Warning("Login attempt with empty username or password");
            return BadRequest(new { message = "Username and password are required" });
        }

        var user = await _userService.AuthenticateAsync(request.Username, request.Password);
        if (user == null)
        {
            Log.Warning("Failed login attempt for username: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = GenerateJwtToken(user);
        Log.Information("Successful login for user: {Username} with role: {Role}", user.Username, user.Role);
        
        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            }
        });
    }

    private string GenerateJwtToken(Core.Entities.User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TaskManagementAPI";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "TaskManagementClient";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username of the user
    /// </summary>
    /// <example>admin</example>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password of the user
    /// </summary>
    /// <example>Admin123!</example>
    public string Password { get; set; } = string.Empty;
}


