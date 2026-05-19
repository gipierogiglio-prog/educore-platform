using FluentValidation;

namespace Giglio.EduCore.Organization.Application.Commands;

public record UpdateSchoolUnitCommand(
    Guid Id,
    string? Name = null,
    string? Address = null,
    string? Number = null,
    string? Neighborhood = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null,
    string? Phone = null,
    string? ResponsibleName = null);

public class UpdateSchoolUnitValidator : AbstractValidator<UpdateSchoolUnitCommand>
{
    public UpdateSchoolUnitValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        });

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(300);
        });

        When(x => x.Number != null, () =>
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                .MaximumLength(20);
        });

        When(x => x.Neighborhood != null, () =>
        {
            RuleFor(x => x.Neighborhood)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.City != null, () =>
        {
            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.State != null, () =>
        {
            RuleFor(x => x.State)
                .NotEmpty()
                .Length(2);
        });

        When(x => x.ZipCode != null, () =>
        {
            RuleFor(x => x.ZipCode)
                .NotEmpty()
                .MaximumLength(10);
        });

        When(x => x.Phone != null, () =>
        {
            RuleFor(x => x.Phone)
                .MaximumLength(20);
        });

        When(x => x.ResponsibleName != null, () =>
        {
            RuleFor(x => x.ResponsibleName)
                .MaximumLength(200);
        });
    }
}