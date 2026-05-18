using FluentValidation;

namespace Giglio.EduCore.Financial.Application.Commands.Payments;

public record CancelPaymentCommand(Guid PaymentId, string Reason);

public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment id is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancel reason is required.")
            .MinimumLength(10)
            .WithMessage("Cancel reason must be at least 10 characters.")
            .MaximumLength(500)
            .WithMessage("Cancel reason must be at most 500 characters.");
    }
}
