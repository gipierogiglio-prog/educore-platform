using FluentValidation;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record CompleteRoutineRecordCommand(
    Guid Id,
    TimeSpan? EndTime = null,
    string? Notes = null);

public class CompleteRoutineRecordValidator : AbstractValidator<CompleteRoutineRecordCommand>
{
    public CompleteRoutineRecordValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}
