using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators;

public sealed class SendProbeEmailRequestValidator : AbstractValidator<SendProbeEmailRequest>
{
    public SendProbeEmailRequestValidator()
    {
        RuleFor(x => x.ToEmail).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200).WithMessage("Konu boş olamaz.");
        RuleFor(x => x.Body).NotEmpty().WithMessage("İçerik boş olamaz.");
    }
}
