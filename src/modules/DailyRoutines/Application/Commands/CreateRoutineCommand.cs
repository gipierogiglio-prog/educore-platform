using FluentValidation;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record CreateRoutineCommand(
    string Name,
    RoutineCategory Category,
    int ExpectedDurationMinutes,
    string? Description = null);

public class CreateRoutineValidator : AbstractValidator<CreateRoutineCommand>
{
    public CreateRoutineValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Name is required and must be at most 200 characters");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Invalid routine category");

        RuleFor(x => x.ExpectedDurationMinutes)
            .GreaterThan(0)
            .WithMessage("Expected duration must be greater than zero")
            .LessThanOrEqualTo(480)
            .WithMessage("Expected duration cannot exceed 480 minutes (8 hours)");
    }
}
