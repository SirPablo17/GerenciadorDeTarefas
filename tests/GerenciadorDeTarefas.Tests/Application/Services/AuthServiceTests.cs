using GerenciadorDeTarefas.Application.Abstractions;
using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Exceptions;
using GerenciadorDeTarefas.Application.Services;
using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;
using Moq;

namespace GerenciadorDeTarefas.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepository.Object, _passwordHasher.Object, _tokenGenerator.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserWithHashedPassword()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash("Password123")).Returns("hashed-password");

        await _sut.RegisterAsync(new RegisterRequest { Email = "new@test.com", Password = "Password123" });

        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "new@test.com" && u.PasswordHash == "hashed-password"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsEmailAlreadyInUseException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "existing@test.com" });

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Email = "existing@test.com", Password = "Password123" }));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", PasswordHash = "hashed-password" };
        _userRepository.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed-password", "Password123")).Returns(true);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        _tokenGenerator.Setup(t => t.GenerateToken(user)).Returns(("jwt-token", expiresAt));

        var result = await _sut.LoginAsync(new LoginRequest { Email = "user@test.com", Password = "Password123" });

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(expiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "unknown@test.com", Password = "Password123" }));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", PasswordHash = "hashed-password" };
        _userRepository.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed-password", "WrongPassword")).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "user@test.com", Password = "WrongPassword" }));
    }
}
