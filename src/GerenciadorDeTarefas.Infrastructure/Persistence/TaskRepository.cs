using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDeTarefas.Infrastructure.Persistence;

public class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Update(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
