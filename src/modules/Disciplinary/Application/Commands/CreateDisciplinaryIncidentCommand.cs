using FluentValidation;

namespace Giglio.EduCore.Disciplinary.Application.Commands;

public record CreateDisciplinaryIncidentCommand(
    Guid StudentId,
    Guid ClassId,
    string Type,
    string Description,
    Guid RecordedById,
    DateTime OccurredAt);

public class CreateDisciplinaryIncidentValidator : AbstractValidator<CreateDisciplinaryIncidentCommand>
{
    public CreateDisciplinaryIncidentValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("StudentId is required");

        RuleFor(x => x.ClassId)
            .NotEmpty()
            .WithMessage("ClassId is required");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => new[] { "warning", "suspension", "occurrence", "expulsion", "other" }.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("Type must be one of: warning, suspension, occurrence, expulsion, other");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Description is required and must be at most 2000 characters");

        RuleFor(x => x.RecordedById)
            .NotEmpty()
            .WithMessage("RecordedById is required");

        RuleFor(x => x.OccurredAt)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("OccurredAt cannot be in the future");
    }
}
