namespace GerenciadorDeTarefas.Application.Exceptions;

public class TaskNotFoundException(Guid taskId)
    : Exception($"Tarefa '{taskId}' não encontrada.");
