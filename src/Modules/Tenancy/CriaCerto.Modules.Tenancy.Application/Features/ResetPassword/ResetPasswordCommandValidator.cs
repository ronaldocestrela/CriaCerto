using FluentValidation;

namespace CriaCerto.Modules.Tenancy.Application.Features.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("O token de redefinição é obrigatório.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("A nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("A nova senha deve ter no mínimo 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches(@"[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches(@"[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
