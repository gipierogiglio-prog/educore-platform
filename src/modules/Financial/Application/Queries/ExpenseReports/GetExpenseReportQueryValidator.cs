using FluentValidation;

namespace Giglio.EduCore.Financial.Application.Queries.ExpenseReports;

public class GetExpenseReportQueryValidator : AbstractValidator<GetExpenseReportQuery>
{
    public GetExpenseReportQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Data início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Data fim é obrigatória.");

        RuleFor(x => x)
            .Must(x => x.EndDate >= x.StartDate)
            .WithMessage("Data início não pode ser posterior à data fim.")
            .WithName("startDate");

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 366)
            .WithMessage("Período máximo é de 12 meses (366 dias).")
            .WithName("period");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status!)
                .Must(s => Enum.TryParse<Domain.Enums.ExpenseStatus>(s, ignoreCase: true, out _))
                .WithMessage("Status inválido. Valores aceitos: Pending, Paid, Overdue, Cancelled.");
        });
    }
}
