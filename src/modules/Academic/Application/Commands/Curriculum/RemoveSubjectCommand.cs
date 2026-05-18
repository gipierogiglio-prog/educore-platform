using FluentValidation;

namespace Giglio.EduCore.Academic.Application.Commands.Curriculum;

public record RemoveSubjectCommand(Guid SeriesId, Guid SubjectId);

public class RemoveSubjectValidator : AbstractValidator<RemoveSubjectCommand>
{
    public RemoveSubjectValidator()
    {
        RuleFor(x => x.SeriesId)
            .NotEmpty()
            .WithMessage("SeriesId is required");

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .WithMessage("SubjectId is required");
    }
}