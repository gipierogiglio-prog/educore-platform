using FluentValidation;

namespace Giglio.EduCore.Academic.Application.Commands.Curriculum;

public record AddSubjectCommand(
    Guid SeriesId,
    Guid SubjectId,
    int WeeklyHours,
    int? TotalHours = null);

public class AddSubjectValidator : AbstractValidator<AddSubjectCommand>
{
    public AddSubjectValidator()
    {
        RuleFor(x => x.SeriesId)
            .NotEmpty()
            .WithMessage("SeriesId is required");

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("SubjectId is required");

        RuleFor(x => x.WeeklyHours)
            .InclusiveBetween(1, 40)
            .WithMessage("Weekly hours must be between 1 and 40");

        When(x => x.TotalHours.HasValue, () =>
        {
            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .WithMessage("Total hours must be greater than zero");
        });
    }
}