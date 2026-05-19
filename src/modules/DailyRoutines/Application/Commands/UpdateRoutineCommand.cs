using FluentValidation;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.Commands;

public record UpdateRoutineCommand(
    Guid Id,
    string? Name = null,
    RoutineCategory? Category = null,
    int? ExpectedDurationMinutes = null,
    string? Description = null);

public class UpdateRoutineValidator : AbstractValidator<UpdateRoutineCommand>
{
    public UpdateRoutineValidator()
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

        When(x => x.Category.HasValue, () =>
        {
            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Invalid routine category");
        });

        When(x => x.ExpectedDurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.ExpectedDurationMinutes)
                .GreaterThan(0)
                .WithMessage("Expected duration must be greater than zero")
                .LessThanOrEqualTo(480)
                .WithMessage("Expected duration cannot exceed 480 minutes");
        });
    }
}
