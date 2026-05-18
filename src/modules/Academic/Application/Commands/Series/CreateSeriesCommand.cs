using FluentValidation;

namespace Giglio.EduCore.Academic.Application.Commands.Series;

public record CreateSeriesCommand(
    string Name,
    Guid CourseId,
    int AcademicYear,
    int? TotalHours = null);

public class CreateSeriesValidator : AbstractValidator<CreateSeriesCommand>
{
    public CreateSeriesValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Name is required and must be at most 200 characters");

        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required");

        RuleFor(x => x.AcademicYear)
            .InclusiveBetween(1900, 2100)
            .WithMessage("Academic year must be between 1900 and 2100");

        When(x => x.TotalHours.HasValue, () =>
        {
            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .WithMessage("Total hours must be greater than zero");
        });
    }
}