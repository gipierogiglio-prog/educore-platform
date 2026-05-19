using Giglio.EduCore.Disciplinary.Application.Commands;
using Giglio.EduCore.Disciplinary.Domain.Entities;
using Giglio.EduCore.Disciplinary.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Disciplinary.Api.Controllers;

[ApiController]
[Route("api/disciplinary")]
[Authorize]
public class DisciplinaryController : ControllerBase
{
    private readonly IDisciplinaryIncidentRepository _repository;

    public DisciplinaryController(IDisciplinaryIncidentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// List all incidents with optional filters (class, type, status, date range).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? classId,
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var filter = classId.HasValue || !string.IsNullOrWhiteSpace(type) ||
                     !string.IsNullOrWhiteSpace(status) || dateFrom.HasValue || dateTo.HasValue
            ? new DisciplinaryFilter(classId, type, status, dateFrom, dateTo)
            : null;

        var incidents = await _repository.GetAllAsync(filter, ct);
        return Ok(incidents);
    }

    /// <summary>
    /// Get incidents by student.
    /// </summary>
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetByStudent(Guid studentId, CancellationToken ct)
    {
        var incidents = await _repository.GetByStudentAsync(studentId, ct);
        return Ok(incidents);
    }

    /// <summary>
    /// Get incidents by class.
    /// </summary>
    [HttpGet("class/{classId:guid}")]
    public async Task<IActionResult> GetByClass(Guid classId, CancellationToken ct)
    {
        var incidents = await _repository.GetByClassAsync(classId, ct);
        return Ok(incidents);
    }

    /// <summary>
    /// Get a single incident by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var incident = await _repository.GetByIdAsync(id, ct);
        if (incident is null)
            return NotFound(new { error = "Disciplinary incident not found" });
        return Ok(incident);
    }

    /// <summary>
    /// Create a new disciplinary incident.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDisciplinaryIncidentCommand command, CancellationToken ct)
    {
        var validator = new CreateDisciplinaryIncidentValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var incident = new DisciplinaryIncident(
            command.StudentId,
            command.ClassId,
            command.Type,
            command.Description,
            command.RecordedById,
            command.OccurredAt);

        await _repository.AddAsync(incident, ct);
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    /// <summary>
    /// Update an existing disciplinary incident.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDisciplinaryIncidentCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var incident = await _repository.GetByIdAsync(id, ct);
        if (incident is null)
            return NotFound(new { error = "Disciplinary incident not found" });

        var validator = new UpdateDisciplinaryIncidentValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        if (command.Type != null)
            incident.SetType(command.Type);

        if (command.Description != null)
            incident.SetDescription(command.Description);

        if (command.Status != null)
            incident.SetStatus(command.Status);

        if (command.Resolution != null)
            incident.SetResolution(command.Resolution);

        if (command.OccurredAt.HasValue)
            incident.SetOccurredAt(command.OccurredAt.Value);

        await _repository.UpdateAsync(incident, ct);
        return Ok(incident);
    }

    /// <summary>
    /// Delete a disciplinary incident.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var incident = await _repository.GetByIdAsync(id, ct);
        if (incident is null)
            return NotFound(new { error = "Disciplinary incident not found" });

        await _repository.DeleteAsync(incident, ct);
        return NoContent();
    }
}
