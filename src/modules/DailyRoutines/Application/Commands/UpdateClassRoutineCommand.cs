using FluentValidation;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record UpdateClassRoutineCommand(
    Guid Id,
    Guid? ClassId = null,
    Guid? RoutineId = null,
    WeekDay? WeekDay = null,
    TimeSpan? StartTime = null,
    int? DurationMinutes = null);

public class UpdateClassRoutineValidator : AbstractValidator<UpdateClassRoutineCommand>
{
    public UpdateClassRoutineValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        When(x => x.ClassId.HasValue, () =>
        {
            RuleFor(x => x.ClassId)
                .NotEmpty()
                .WithMessage("ClassId cannot be empty");
        });

        When(x => x.RoutineId.HasValue, () =>
        {
            RuleFor(x => x.RoutineId)
                .NotEmpty()
                .WithMessage("RoutineId cannot be empty");
        });

        When(x => x.WeekDay.HasValue, () =>
        {
            RuleFor(x => x.WeekDay)
                .IsInEnum()
                .WithMessage("Invalid week day");
        });

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than zero")
                .LessThanOrEqualTo(480)
                .WithMessage("Duration cannot exceed 480 minutes");
        });
    }
}
