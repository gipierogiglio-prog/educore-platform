using FluentValidation;
using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Application.Commands.Payments;

public record RegisterPaymentCommand(
    Guid MonthlyChargeId,
    decimal Value,
    DateTime PaymentDate,
    string Method,
    string? Observation)
{
    public PaymentMethod? ResolvedMethod => Method switch
    {
        "Cash" => PaymentMethod.Cash,
        "Pix" => PaymentMethod.Pix,
        "TED" => PaymentMethod.TED,
        "BankSlip" => PaymentMethod.BankSlip,
        "CreditCard" => PaymentMethod.CreditCard,
        "DebitCard" => PaymentMethod.DebitCard,
        _ => null
    };
}

public class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
    public RegisterPaymentCommandValidator()
    {
        RuleFor(x => x.MonthlyChargeId)
            .NotEmpty()
            .WithMessage("Monthly charge is required.");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Payment value must be greater than zero.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("Payment date is required.");

        RuleFor(x => x.Method)
            .NotEmpty()
            .WithMessage("Payment method is required.")
            .Must(m => m is "Cash" or "Pix" or "TED" or "BankSlip" or "CreditCard" or "DebitCard")
            .WithMessage("Invalid payment method. Valid values: Cash, Pix, TED, BankSlip, CreditCard, DebitCard.");

        When(x => x.Observation != null, () =>
        {
            RuleFor(x => x.Observation!)
                .MaximumLength(1000)
                .WithMessage("Observation must be at most 1000 characters.");
        });
    }
}
