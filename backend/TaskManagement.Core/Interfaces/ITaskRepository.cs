using TaskManagement.Core.Entities;
using TaskEntity = TaskManagement.Core.Entities.Task;

namespace TaskManagement.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetTasksByUserIdAsync(int userId);
}


