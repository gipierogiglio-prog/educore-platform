using Giglio.EduCore.DailyRoutines.Application.Commands;
using Giglio.EduCore.DailyRoutines.Application.DTOs;
using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.DailyRoutines.Api.Controllers;

/// <summary>
/// CRUD de rotinas diárias (Task #199).
/// </summary>
[ApiController]
[Route("api/daily-routines/routines")]
public class RoutinesController : ControllerBase
{
    private readonly IRoutineRepository _repository;

    public RoutinesController(IRoutineRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? activeOnly,
        [FromQuery] RoutineCategory? category,
        CancellationToken ct)
    {
        var routines = await _repository.GetAllAsync(activeOnly, category, ct);
        var dtos = routines.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var routine = await _repository.GetByIdAsync(id, ct);
        if (routine is null)
            return NotFound(new { error = "Routine not found" });
        return Ok(MapToDto(routine));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoutineCommand command, CancellationToken ct)
    {
        var validator = new CreateRoutineValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var routine = new Routine(
            command.Name,
            command.Category,
            command.ExpectedDurationMinutes,
            command.Description);

        await _repository.AddAsync(routine, ct);
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, MapToDto(routine));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoutineCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var routine = await _repository.GetByIdAsync(id, ct);
        if (routine is null)
            return NotFound(new { error = "Routine not found" });

        if (command.Name != null)
            routine.SetName(command.Name);

        if (command.Category.HasValue)
            routine.SetCategory(command.Category.Value);

        if (command.ExpectedDurationMinutes.HasValue)
            routine.SetExpectedDuration(command.ExpectedDurationMinutes.Value);

        if (command.Description != null)
            routine.SetDescription(command.Description);

        await _repository.UpdateAsync(routine, ct);
        return Ok(MapToDto(routine));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var routine = await _repository.GetByIdAsync(id, ct);
        if (routine is null)
            return NotFound(new { error = "Routine not found" });

        routine.Deactivate();
        await _repository.UpdateAsync(routine, ct);
        return NoContent();
    }

    private static RoutineDto MapToDto(Routine r) => new(
        r.Id, r.Name, r.Description, r.Category,
        r.ExpectedDurationMinutes, r.IsActive, r.CreatedAt, r.UpdatedAt);
}
