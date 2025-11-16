using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskEntity = TaskManagement.Core.Entities.Task;

namespace TaskManagement.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async System.Threading.Tasks.Task<TaskEntity?> GetTaskByIdAsync(int id, int? userId = null, string? role = null)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            return null;

        // If user is not admin, they can only view their own tasks
        if (role != "Admin" && userId.HasValue && task.AssignedUserId != userId.Value)
            return null;

        return task;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetAllTasksAsync(int? userId = null, string? role = null)
    {
        // Admin can see all tasks, regular users can only see their assigned tasks
        if (role == "Admin")
            return await _taskRepository.GetAllAsync();
        
        if (userId.HasValue)
            return await _taskRepository.GetTasksByUserIdAsync(userId.Value);

        return Enumerable.Empty<TaskEntity>();
    }

    public async System.Threading.Tasks.Task<TaskEntity> CreateTaskAsync(TaskEntity task)
    {
        // Verify assigned user exists
        var user = await _userRepository.GetByIdAsync(task.AssignedUserId);
        if (user == null)
            throw new InvalidOperationException("Assigned user not found");

        task.CreatedAt = DateTime.UtcNow;
        return await _taskRepository.AddAsync(task);
    }

    public async System.Threading.Tasks.Task<TaskEntity> UpdateTaskAsync(TaskEntity task, int? userId = null, string? role = null)
    {
        var existingTask = await _taskRepository.GetByIdAsync(task.Id);
        if (existingTask == null)
            throw new InvalidOperationException("Task not found");

        // If user is not admin, they can only update the status of their assigned tasks
        if (role != "Admin")
        {
            if (!userId.HasValue || existingTask.AssignedUserId != userId.Value)
                throw new UnauthorizedAccessException("You can only update your own tasks");

            // Regular users can only update status
            existingTask.Status = task.Status;
            existingTask.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Admin can update all fields
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.AssignedUserId = task.AssignedUserId;
            existingTask.UpdatedAt = DateTime.UtcNow;

            // Verify assigned user exists if changed
            if (existingTask.AssignedUserId != task.AssignedUserId)
            {
                var user = await _userRepository.GetByIdAsync(task.AssignedUserId);
                if (user == null)
                    throw new InvalidOperationException("Assigned user not found");
            }
        }

        return await _taskRepository.UpdateAsync(existingTask);
    }

    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }
}


