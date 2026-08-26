using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Validators;

namespace GerenciadorDeTarefas.Tests.Application.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _sut.Validate(new LoginRequest { Email = "user@test.com", Password = "anything" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "anything")]
    [InlineData("not-an-email", "anything")]
    [InlineData("user@test.com", "")]
    public void Validate_WithInvalidRequest_HasErrors(string email, string password)
    {
        var result = _sut.Validate(new LoginRequest { Email = email, Password = password });

        Assert.False(result.IsValid);
    }
}
