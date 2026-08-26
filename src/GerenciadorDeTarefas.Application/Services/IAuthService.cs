using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Application.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
