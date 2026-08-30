namespace GerenciadorDeTarefas.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int NextTaskNumber { get; set; } = 1;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
