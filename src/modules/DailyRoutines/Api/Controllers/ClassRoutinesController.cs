using Giglio.EduCore.DailyRoutines.Application.Commands;
using Giglio.EduCore.DailyRoutines.Application.DTOs;
using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.DailyRoutines.Api.Controllers;

/// <summary>
/// Lançamento de rotinas por turma (Task #201).
/// </summary>
[ApiController]
[Route("api/daily-routines/class-routines")]
public class ClassRoutinesController : ControllerBase
{
    private readonly IClassRoutineRepository _repository;
    private readonly IRoutineRepository _routineRepository;

    public ClassRoutinesController(
        IClassRoutineRepository repository,
        IRoutineRepository routineRepository)
    {
        _repository = repository;
        _routineRepository = routineRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? classId,
        [FromQuery] WeekDay? weekDay,
        CancellationToken ct)
    {
        if (!classId.HasValue)
            return BadRequest(new { error = "classId is required" });

        var items = await _repository.GetByClassAsync(classId.Value, weekDay, ct);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound(new { error = "Class routine not found" });
        return Ok(MapToDto(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRoutineCommand command, CancellationToken ct)
    {
        var validator = new CreateClassRoutineValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var classRoutine = new ClassRoutine(
            command.ClassId,
            command.RoutineId,
            command.WeekDay,
            command.StartTime,
            command.DurationMinutes);

        await _repository.AddAsync(classRoutine, ct);
        return CreatedAtAction(nameof(GetById), new { id = classRoutine.Id }, MapToDto(classRoutine));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRoutineCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var classRoutine = await _repository.GetByIdAsync(id, ct);
        if (classRoutine is null)
            return NotFound(new { error = "Class routine not found" });

        if (command.ClassId.HasValue)
            classRoutine.SetClass(command.ClassId.Value);

        if (command.RoutineId.HasValue)
            classRoutine.SetRoutine(command.RoutineId.Value);

        if (command.WeekDay.HasValue)
            classRoutine.SetWeekDay(command.WeekDay.Value);

        if (command.StartTime.HasValue)
            classRoutine.SetStartTime(command.StartTime.Value);

        if (command.DurationMinutes.HasValue)
            classRoutine.SetDuration(command.DurationMinutes.Value);

        await _repository.UpdateAsync(classRoutine, ct);
        return Ok(MapToDto(classRoutine));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var classRoutine = await _repository.GetByIdAsync(id, ct);
        if (classRoutine is null)
            return NotFound(new { error = "Class routine not found" });

        classRoutine.Deactivate();
        await _repository.UpdateAsync(classRoutine, ct);
        return NoContent();
    }

    private static ClassRoutineDto MapToDto(ClassRoutine cr) => new(
        cr.Id, cr.ClassId, cr.RoutineId,
        cr.Routine?.Name,
        cr.Routine?.Category.ToString(),
        cr.WeekDay, cr.StartTime, cr.DurationMinutes,
        cr.IsActive, cr.CreatedAt, cr.UpdatedAt);
}
