using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskEntity = TaskManagement.Core.Entities.Task;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskEntity>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetTasksByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(t => t.AssignedUser)
            .Where(t => t.AssignedUserId == userId)
            .ToListAsync();
    }

    public override async System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public override async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }
}


