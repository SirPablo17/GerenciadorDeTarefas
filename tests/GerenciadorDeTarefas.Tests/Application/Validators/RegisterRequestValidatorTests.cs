using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Validators;

namespace GerenciadorDeTarefas.Tests.Application.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _sut.Validate(new RegisterRequest { Email = "user@test.com", Password = "Password123" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Password123")]
    [InlineData("not-an-email", "Password123")]
    [InlineData("user@test.com", "")]
    [InlineData("user@test.com", "short1A")]
    [InlineData("user@test.com", "alllowercase1")]
    [InlineData("user@test.com", "ALLUPPERCASE1")]
    [InlineData("user@test.com", "NoDigitsHere")]
    public void Validate_WithInvalidRequest_HasErrors(string email, string password)
    {
        var result = _sut.Validate(new RegisterRequest { Email = email, Password = password });

        Assert.False(result.IsValid);
    }
}
