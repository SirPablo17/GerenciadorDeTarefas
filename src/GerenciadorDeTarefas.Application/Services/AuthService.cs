using GerenciadorDeTarefas.Application.Abstractions;
using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Exceptions;
using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;

namespace GerenciadorDeTarefas.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IAuthService
{
    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new EmailAlreadyInUseException(request.Email);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        var (token, expiresAt) = tokenGenerator.GenerateToken(user);
        return new AuthResponse { Token = token, ExpiresAt = expiresAt };
    }
}
