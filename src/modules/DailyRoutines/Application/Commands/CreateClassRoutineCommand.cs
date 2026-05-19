using FluentValidation;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record CreateClassRoutineCommand(
    Guid ClassId,
    Guid RoutineId,
    WeekDay WeekDay,
    TimeSpan StartTime,
    int DurationMinutes);

public class CreateClassRoutineValidator : AbstractValidator<CreateClassRoutineCommand>
{
    public CreateClassRoutineValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty()
            .WithMessage("ClassId is required");

        RuleFor(x => x.RoutineId)
            .NotEmpty()
            .WithMessage("RoutineId is required");

        RuleFor(x => x.WeekDay)
            .IsInEnum()
            .WithMessage("Invalid week day");

        RuleFor(x => x.StartTime)
            .NotNull()
            .WithMessage("Start time is required");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than zero")
            .LessThanOrEqualTo(480)
            .WithMessage("Duration cannot exceed 480 minutes");
    }
}
