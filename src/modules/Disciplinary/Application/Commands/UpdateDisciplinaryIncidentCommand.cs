using FluentValidation;

namespace Giglio.EduCore.Disciplinary.Application.Commands;

public record UpdateDisciplinaryIncidentCommand(
    Guid Id,
    string? Type = null,
    string? Description = null,
    string? Status = null,
    string? Resolution = null,
    DateTime? OccurredAt = null);

public class UpdateDisciplinaryIncidentValidator : AbstractValidator<UpdateDisciplinaryIncidentCommand>
{
    public UpdateDisciplinaryIncidentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        When(x => x.Type != null, () =>
        {
            RuleFor(x => x.Type)
                .Must(t => new[] { "warning", "suspension", "occurrence", "expulsion", "other" }.Contains(t.Trim().ToLowerInvariant()))
                .WithMessage("Type must be one of: warning, suspension, occurrence, expulsion, other");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(2000)
                .WithMessage("Description must be at most 2000 characters");
        });

        When(x => x.Status != null, () =>
        {
            RuleFor(x => x.Status)
                .Must(s => new[] { "pending", "resolved", "dismissed" }.Contains(s.Trim().ToLowerInvariant()))
                .WithMessage("Status must be one of: pending, resolved, dismissed");
        });

        When(x => x.OccurredAt.HasValue, () =>
        {
            RuleFor(x => x.OccurredAt)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("OccurredAt cannot be in the future");
        });
    }
}
