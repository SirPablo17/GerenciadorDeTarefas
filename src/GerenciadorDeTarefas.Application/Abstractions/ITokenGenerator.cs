using GerenciadorDeTarefas.Domain.Entities;

namespace GerenciadorDeTarefas.Application.Abstractions;

public interface ITokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
