using FluentValidation;

namespace Giglio.EduCore.Financial.Application.Commands.Plans;

public record CreateFinancialPlanCommand(
    Guid EnrollmentId,
    decimal BaseValue,
    int DueDay,
    int StartMonth,
    int StartYear,
    decimal? DiscountPercent = null,
    string? DiscountType = null,
    int? EndMonth = null,
    int? EndYear = null)
{
    public Domain.Enums.DiscountType? ResolvedDiscountType => DiscountType switch
    {
        "Percentage" => Domain.Enums.DiscountType.Percentage,
        "Fixed" => Domain.Enums.DiscountType.Fixed,
        _ => null
    };
}

public class CreateFinancialPlanValidator : AbstractValidator<CreateFinancialPlanCommand>
{
    public CreateFinancialPlanValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty();

        RuleFor(x => x.BaseValue)
            .GreaterThan(0)
            .WithMessage("Base value must be greater than zero");

        RuleFor(x => x.DueDay)
            .InclusiveBetween(1, 31)
            .WithMessage("Due day must be between 1 and 31");

        RuleFor(x => x.StartMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Start month must be between 1 and 12");

        RuleFor(x => x.StartYear)
            .GreaterThanOrEqualTo(2020)
            .WithMessage("Invalid start year");

        When(x => x.DiscountPercent.HasValue, () =>
        {
            RuleFor(x => x.DiscountType)
                .NotEmpty()
                .WithMessage("Discount type is required when discount percent is provided");

            RuleFor(x => x.DiscountPercent)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount cannot be negative");

            When(x => x.DiscountType == "Fixed", () =>
            {
                RuleFor(x => x.DiscountPercent)
                    .LessThanOrEqualTo(x => x.BaseValue)
                    .WithMessage("Fixed discount cannot exceed base value");
            });

            When(x => x.DiscountType == "Percentage", () =>
            {
                RuleFor(x => x.DiscountPercent)
                    .LessThanOrEqualTo(100)
                    .WithMessage("Percentage discount cannot exceed 100%");
            });
        });

        When(x => x.EndYear.HasValue, () =>
        {
            RuleFor(x => x.EndMonth)
                .NotNull()
                .WithMessage("End month is required when end year is provided")
                .InclusiveBetween(1, 12);

            RuleFor(x => x)
                .Must(x => !x.EndYear.HasValue || x.EndYear > x.StartYear
                    || (x.EndYear == x.StartYear && x.EndMonth > x.StartMonth))
                .WithMessage("End period must be after start period");
        });
    }
}
