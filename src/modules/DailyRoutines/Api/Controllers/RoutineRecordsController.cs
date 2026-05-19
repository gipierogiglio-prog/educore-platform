using Giglio.EduCore.DailyRoutines.Application.Commands;
using Giglio.EduCore.DailyRoutines.Application.DTOs;
using Giglio.EduCore.DailyRoutines.Application.Queries;
using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.DailyRoutines.Api.Controllers;

/// <summary>
/// Registro de rotinas diárias - lançamento de execução.
/// </summary>
[ApiController]
[Route("api/daily-routines/records")]
public class RoutineRecordsController : ControllerBase
{
    private readonly IDailyRoutineRecordRepository _repository;
    private readonly GetClassRoutinesForDateQuery _getRoutinesQuery;

    public RoutineRecordsController(
        IDailyRoutineRecordRepository repository,
        GetClassRoutinesForDateQuery getRoutinesQuery)
    {
        _repository = repository;
        _getRoutinesQuery = getRoutinesQuery;
    }

    [HttpGet("by-class-date")]
    public async Task<IActionResult> GetByClassAndDate(
        [FromQuery] Guid classId,
        [FromQuery] DateTime date,
        CancellationToken ct)
    {
        var records = await _getRoutinesQuery.ExecuteAsync(classId, date, ct);
        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var record = await _repository.GetByIdAsync(id, ct);
        if (record is null)
            return NotFound(new { error = "Record not found" });
        return Ok(MapToDto(record));
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartRoutineRecordCommand command, CancellationToken ct)
    {
        var validator = new StartRoutineRecordValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        // Check if already exists for this class routine + date
        var existing = await _repository.GetByClassRoutineAndDateAsync(
            command.ClassRoutineId, command.RecordDate, ct);

        if (existing != null)
        {
            if (existing.Status == RoutineRecordStatus.InProgress)
                return Conflict(new { error = "Routine already in progress", recordId = existing.Id });

            if (existing.Status == RoutineRecordStatus.Completed)
                return Conflict(new { error = "Routine already completed", recordId = existing.Id });

            // If cancelled or pending, update instead
            existing.Start(command.StartTime, command.TeacherId);
            await _repository.UpdateAsync(existing, ct);
            return Ok(MapToDto(existing));
        }

        var record = new DailyRoutineRecord(command.ClassRoutineId, command.RecordDate);
        record.Start(command.StartTime, command.TeacherId);

        await _repository.AddAsync(record, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, MapToDto(record));
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteRoutineRecordCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var record = await _repository.GetByIdAsync(id, ct);
        if (record is null)
            return NotFound(new { error = "Record not found" });

        record.Complete(command.EndTime, command.Notes);
        await _repository.UpdateAsync(record, ct);
        return Ok(MapToDto(record));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason, CancellationToken ct)
    {
        var record = await _repository.GetByIdAsync(id, ct);
        if (record is null)
            return NotFound(new { error = "Record not found" });

        record.Cancel(reason);
        await _repository.UpdateAsync(record, ct);
        return Ok(MapToDto(record));
    }

    private static DailyRoutineRecordDto MapToDto(DailyRoutineRecord r) => new(
        r.Id, r.ClassRoutineId,
        r.ClassRoutine?.Routine?.Name,
        r.ClassRoutine?.Routine?.Category.ToString(),
        r.RecordDate, r.StartTime, r.EndTime,
        r.Status, r.Notes, r.TeacherId,
        r.CreatedAt, r.UpdatedAt);
}
