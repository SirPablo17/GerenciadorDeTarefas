using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Exceptions;
using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;

namespace GerenciadorDeTarefas.Application.Services;

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<TaskDto> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await taskRepository.AddAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task<IReadOnlyList<TaskDto>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await taskRepository.ListByUserAsync(userId, cancellationToken);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        return ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        await taskRepository.DeleteAsync(task, cancellationToken);
    }

    private async Task<TaskItem> GetOwnedTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null || task.UserId != userId)
        {
            throw new TaskNotFoundException(taskId);
        }

        return task;
    }

    private static TaskDto ToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
