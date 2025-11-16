using TaskManagement.Core.Entities;
using TaskEntity = TaskManagement.Core.Entities.Task;

namespace TaskManagement.Core.Interfaces;

public interface ITaskService
{
    System.Threading.Tasks.Task<TaskEntity?> GetTaskByIdAsync(int id, int? userId = null, string? role = null);
    System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetAllTasksAsync(int? userId = null, string? role = null);
    System.Threading.Tasks.Task<TaskEntity> CreateTaskAsync(TaskEntity task);
    System.Threading.Tasks.Task<TaskEntity> UpdateTaskAsync(TaskEntity task, int? userId = null, string? role = null);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
}


