using GerenciadorDeTarefas.Domain.Entities;

namespace GerenciadorDeTarefas.Application.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
}
