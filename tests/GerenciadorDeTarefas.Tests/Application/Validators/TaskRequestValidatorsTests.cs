using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Validators;
using GerenciadorDeTarefas.Domain.Entities;

namespace GerenciadorDeTarefas.Tests.Application.Validators;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _sut.Validate(new CreateTaskRequest { Title = "Comprar pão" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasErrors()
    {
        var result = _sut.Validate(new CreateTaskRequest { Title = "" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithTitleTooLong_HasErrors()
    {
        var result = _sut.Validate(new CreateTaskRequest { Title = new string('a', 201) });

        Assert.False(result.IsValid);
    }
}

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _sut.Validate(new UpdateTaskRequest { Title = "Comprar pão", Status = TaskItemStatus.InProgress });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasErrors()
    {
        var result = _sut.Validate(new UpdateTaskRequest { Title = "" });

        Assert.False(result.IsValid);
    }
}
