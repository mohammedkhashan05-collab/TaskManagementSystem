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
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Gets all tasks. Admins see all tasks, regular users see only their assigned tasks.
    /// </summary>
    /// <returns>List of tasks based on user role</returns>
    /// <response code="200">Returns the list of tasks</response>
    /// <response code="401">If user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        var tasks = await _taskService.GetAllTasksAsync(userId, role);
        return Ok(tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            AssignedUserId = t.AssignedUserId,
            AssignedUserName = t.AssignedUser?.Username ?? string.Empty,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }));
    }

    /// <summary>
    /// Gets a specific task by ID. Regular users can only view their own tasks.
    /// </summary>
    /// <param name="id">The ID of the task to retrieve</param>
    /// <returns>The task details</returns>
    /// <response code="200">Returns the task</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user tries to access a task that doesn't belong to them</response>
    /// <response code="404">If task is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        var task = await _taskService.GetTaskByIdAsync(id, userId, role);
        if (task == null)
            return NotFound(new { message = "Task not found" });

        return Ok(new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            AssignedUserId = task.AssignedUserId,
            AssignedUserName = task.AssignedUser?.Username ?? string.Empty,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        });
    }

    /// <summary>
    /// Creates a new task. Only admins can create tasks.
    /// </summary>
    /// <param name="request">Task creation request containing title, description, status, and assigned user ID</param>
    /// <returns>The created task</returns>
    /// <response code="201">Returns the newly created task</response>
    /// <response code="400">If the request is invalid or title is missing</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an admin</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        Log.Information("User {UserId} attempting to create task: {Title}", userId, request.Title);

        if (string.IsNullOrEmpty(request.Title))
        {
            Log.Warning("Task creation failed: Title is required");
            return BadRequest(new { message = "Title is required" });
        }

        try
        {
            var task = new Core.Entities.Task
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                Status = request.Status ?? "Pending",
                AssignedUserId = request.AssignedUserId
            };

            var createdTask = await _taskService.CreateTaskAsync(task);
            Log.Information("Task created successfully. TaskId: {TaskId}, Title: {Title}, AssignedTo: {AssignedUserId}", 
                createdTask.Id, createdTask.Title, createdTask.AssignedUserId);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, new TaskDto
            {
                Id = createdTask.Id,
                Title = createdTask.Title,
                Description = createdTask.Description,
                Status = createdTask.Status,
                AssignedUserId = createdTask.AssignedUserId,
                AssignedUserName = createdTask.AssignedUser?.Username ?? string.Empty,
                CreatedAt = createdTask.CreatedAt,
                UpdatedAt = createdTask.UpdatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Error creating task: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing task. Admins can update all fields, regular users can only update the status of their assigned tasks.
    /// </summary>
    /// <param name="id">The ID of the task to update</param>
    /// <param name="request">Task update request containing fields to update</param>
    /// <returns>The updated task</returns>
    /// <response code="200">Returns the updated task</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user tries to update a task they don't have permission to modify</response>
    /// <response code="404">If task is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        Log.Information("User {UserId} (Role: {Role}) attempting to update task {TaskId}", userId, role, id);

        try
        {
            var existingTask = await _taskService.GetTaskByIdAsync(id, userId, role);
            if (existingTask == null)
                return NotFound(new { message = "Task not found" });

            var task = new Core.Entities.Task
            {
                Id = id,
                Title = request.Title ?? existingTask.Title,
                Description = request.Description ?? existingTask.Description,
                Status = request.Status ?? existingTask.Status,
                AssignedUserId = request.AssignedUserId ?? existingTask.AssignedUserId
            };

            var updatedTask = await _taskService.UpdateTaskAsync(task, userId, role);
            return Ok(new TaskDto
            {
                Id = updatedTask.Id,
                Title = updatedTask.Title,
                Description = updatedTask.Description,
                Status = updatedTask.Status,
                AssignedUserId = updatedTask.AssignedUserId,
                AssignedUserName = updatedTask.AssignedUser?.Username ?? string.Empty,
                CreatedAt = updatedTask.CreatedAt,
                UpdatedAt = updatedTask.UpdatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning("Unauthorized task update attempt. User: {UserId}, Task: {TaskId}, Error: {Message}", userId, id, ex.Message);
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Error updating task {TaskId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task by ID. Only admins can delete tasks.
    /// </summary>
    /// <param name="id">The ID of the task to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="401">If user is not authenticated</response>
    /// <response code="403">If user is not an admin</response>
    /// <response code="404">If task is not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetCurrentUserId();
        Log.Information("Admin user {UserId} attempting to delete task {TaskId}", userId, id);

        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
        {
            Log.Warning("Task {TaskId} not found for deletion", id);
            return NotFound(new { message = "Task not found" });
        }

        Log.Information("Task {TaskId} deleted successfully by user {UserId}", id, userId);
        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }

    private string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }
}

/// <summary>
/// Task data transfer object
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Unique identifier of the task
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Title of the task
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the task
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the task (e.g., Pending, InProgress, Completed)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the user assigned to this task
    /// </summary>
    public int AssignedUserId { get; set; }
    
    /// <summary>
    /// Username of the user assigned to this task
    /// </summary>
    public string AssignedUserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the task was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new task
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Title of the task (required)
    /// </summary>
    /// <example>Implement new feature</example>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the task
    /// </summary>
    /// <example>Add user authentication functionality</example>
    public string? Description { get; set; }
    
    /// <summary>
    /// Status of the task (defaults to "Pending" if not provided)
    /// </summary>
    /// <example>Pending</example>
    public string? Status { get; set; }
    
    /// <summary>
    /// ID of the user to assign this task to
    /// </summary>
    /// <example>1</example>
    public int AssignedUserId { get; set; }
}

/// <summary>
/// Request model for updating an existing task
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// New title for the task
    /// </summary>
    /// <example>Updated task title</example>
    public string? Title { get; set; }
    
    /// <summary>
    /// New description for the task
    /// </summary>
    /// <example>Updated task description</example>
    public string? Description { get; set; }
    
    /// <summary>
    /// New status for the task
    /// </summary>
    /// <example>InProgress</example>
    public string? Status { get; set; }
    
    /// <summary>
    /// ID of the user to reassign this task to
    /// </summary>
    /// <example>2</example>
    public int? AssignedUserId { get; set; }
}


