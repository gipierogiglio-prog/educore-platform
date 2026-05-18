using FluentValidation;

namespace Giglio.EduCore.Financial.Application.Commands.Plans;

public record UpdateFinancialPlanCommand(
    Guid Id,
    decimal? BaseValue = null,
    int? DueDay = null,
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

public class UpdateFinancialPlanValidator : AbstractValidator<UpdateFinancialPlanCommand>
{
    public UpdateFinancialPlanValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        When(x => x.BaseValue.HasValue, () =>
        {
            RuleFor(x => x.BaseValue)
                .GreaterThan(0);
        });

        When(x => x.DueDay.HasValue, () =>
        {
            RuleFor(x => x.DueDay)
                .InclusiveBetween(1, 31);
        });

        When(x => x.DiscountPercent.HasValue, () =>
        {
            RuleFor(x => x.DiscountPercent)
                .GreaterThanOrEqualTo(0);
        });
    }
}
