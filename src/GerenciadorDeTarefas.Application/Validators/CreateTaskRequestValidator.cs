using FluentValidation;
using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
