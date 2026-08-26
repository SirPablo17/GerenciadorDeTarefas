using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Application.Services;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskDto>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}
