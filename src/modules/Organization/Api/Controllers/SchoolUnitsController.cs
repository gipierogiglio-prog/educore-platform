using Giglio.EduCore.Organization.Application.Commands;
using Giglio.EduCore.Organization.Domain.Entities;
using Giglio.EduCore.Organization.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Organization.Api.Controllers;

[ApiController]
[Route("api/organization/units")]
public class SchoolUnitsController : ControllerBase
{
    private readonly ISchoolUnitRepository _repository;

    public SchoolUnitsController(ISchoolUnitRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var units = await _repository.GetAllAsync(activeOnly, ct);
        return Ok(units);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var unit = await _repository.GetByIdAsync(id, ct);
        if (unit is null)
            return NotFound(new { error = "School unit not found" });
        return Ok(unit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSchoolUnitCommand command, CancellationToken ct)
    {
        var validator = new CreateSchoolUnitValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var unit = new SchoolUnit(
            command.Name,
            command.Address,
            command.Number,
            command.Neighborhood,
            command.City,
            command.State,
            command.ZipCode,
            command.Phone,
            command.ResponsibleName);

        await _repository.AddAsync(unit, ct);
        return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolUnitCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var unit = await _repository.GetByIdAsync(id, ct);
        if (unit is null)
            return NotFound(new { error = "School unit not found" });

        if (command.Name != null) unit.SetName(command.Name);
        if (command.Address != null) unit.SetAddress(command.Address);
        if (command.Number != null) unit.SetNumber(command.Number);
        if (command.Neighborhood != null) unit.SetNeighborhood(command.Neighborhood);
        if (command.City != null) unit.SetCity(command.City);
        if (command.State != null) unit.SetState(command.State);
        if (command.ZipCode != null) unit.SetZipCode(command.ZipCode);
        if (command.Phone != null) unit.SetPhone(command.Phone);
        if (command.ResponsibleName != null) unit.SetResponsibleName(command.ResponsibleName);

        await _repository.UpdateAsync(unit, ct);
        return Ok(unit);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var unit = await _repository.GetByIdAsync(id, ct);
        if (unit is null)
            return NotFound(new { error = "School unit not found" });

        // Soft delete: deactivate instead of hard delete
        unit.Deactivate();
        await _repository.UpdateAsync(unit, ct);
        return NoContent();
    }
}