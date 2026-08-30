using GerenciadorDeTarefas.Domain.Entities;

namespace GerenciadorDeTarefas.Application.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
