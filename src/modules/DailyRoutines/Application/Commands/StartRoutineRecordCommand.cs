using FluentValidation;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record StartRoutineRecordCommand(
    Guid ClassRoutineId,
    DateTime RecordDate,
    TimeSpan StartTime,
    Guid? TeacherId = null);

public class StartRoutineRecordValidator : AbstractValidator<StartRoutineRecordCommand>
{
    public StartRoutineRecordValidator()
    {
        RuleFor(x => x.ClassRoutineId)
            .NotEmpty()
            .WithMessage("ClassRoutineId is required");

        RuleFor(x => x.RecordDate)
            .NotEmpty()
            .WithMessage("RecordDate is required");

        RuleFor(x => x.StartTime)
            .NotNull()
            .WithMessage("StartTime is required");
    }
}
