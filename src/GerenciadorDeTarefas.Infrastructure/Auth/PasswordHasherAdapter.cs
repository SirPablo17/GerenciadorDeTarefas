using GerenciadorDeTarefas.Application.Abstractions;
using GerenciadorDeTarefas.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GerenciadorDeTarefas.Infrastructure.Auth;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(user: null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user: null!, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
