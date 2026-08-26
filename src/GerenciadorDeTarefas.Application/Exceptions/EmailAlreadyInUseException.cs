namespace GerenciadorDeTarefas.Application.Exceptions;

public class EmailAlreadyInUseException(string email)
    : Exception($"O email '{email}' já está em uso.");
