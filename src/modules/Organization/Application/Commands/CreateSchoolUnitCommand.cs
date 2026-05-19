using FluentValidation;

namespace Giglio.EduCore.Organization.Application.Commands;

public record CreateSchoolUnitCommand(
    string Name,
    string Address,
    string Number,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string? Phone = null,
    string? ResponsibleName = null);

public class CreateSchoolUnitValidator : AbstractValidator<CreateSchoolUnitCommand>
{
    public CreateSchoolUnitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Name is required (max 200 chars)");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300)
            .WithMessage("Address is required (max 300 chars)");

        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Number is required (max 20 chars)");

        RuleFor(x => x.Neighborhood)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Neighborhood is required (max 100 chars)");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("City is required (max 100 chars)");

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2)
            .WithMessage("State is required (2-letter code)");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .MaximumLength(10)
            .WithMessage("ZipCode is required (max 10 chars)");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone != null);

        RuleFor(x => x.ResponsibleName)
            .MaximumLength(200)
            .When(x => x.ResponsibleName != null);
    }
}