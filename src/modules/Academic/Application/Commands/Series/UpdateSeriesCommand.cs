using FluentValidation;

namespace Giglio.EduCore.Academic.Application.Commands.Series;

public record UpdateSeriesCommand(
    Guid Id,
    string? Name = null,
    Guid? CourseId = null,
    int? AcademicYear = null,
    int? TotalHours = null);

public class UpdateSeriesValidator : AbstractValidator<UpdateSeriesCommand>
{
    public UpdateSeriesValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Name must be at most 200 characters");
        });

        When(x => x.AcademicYear.HasValue, () =>
        {
            RuleFor(x => x.AcademicYear)
                .InclusiveBetween(1900, 2100)
                .WithMessage("Academic year must be between 1900 and 2100");
        });

        When(x => x.TotalHours.HasValue, () =>
        {
            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .WithMessage("Total hours must be greater than zero");
        });
    }
}