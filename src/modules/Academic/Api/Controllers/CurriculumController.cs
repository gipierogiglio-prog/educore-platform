using Giglio.EduCore.Academic.Application.Commands.Curriculum;
using Giglio.EduCore.Academic.Domain.Entities;
using Giglio.EduCore.Academic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Academic.Api.Controllers;

[ApiController]
[Route("api/academic/series/{seriesId:guid}/curriculum")]
public class CurriculumController : ControllerBase
{
    private readonly ICurriculumMatrixRepository _curriculumRepository;
    private readonly ISeriesRepository _seriesRepository;

    public CurriculumController(
        ICurriculumMatrixRepository curriculumRepository,
        ISeriesRepository seriesRepository)
    {
        _curriculumRepository = curriculumRepository;
        _seriesRepository = seriesRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid seriesId, CancellationToken ct)
    {
        var seriesExists = await _seriesRepository.ExistsAsync(seriesId, ct);
        if (!seriesExists)
            return NotFound(new { error = "Series not found" });

        var entries = await _curriculumRepository.GetBySeriesAsync(seriesId, ct);
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> AddSubject(Guid seriesId, [FromBody] AddSubjectCommand command, CancellationToken ct)
    {
        if (seriesId != command.SeriesId)
            return BadRequest(new { error = "SeriesId mismatch" });

        var validator = new AddSubjectValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var seriesExists = await _seriesRepository.ExistsAsync(seriesId, ct);
        if (!seriesExists)
            return NotFound(new { error = "Series not found" });

        var alreadyExists = await _curriculumRepository.SubjectExistsInSeriesAsync(seriesId, command.SubjectId, ct);
        if (alreadyExists)
            return Conflict(new { error = "Subject already added to this series" });

        var entry = new CurriculumMatrix(
            command.SeriesId,
            command.SubjectId,
            command.WeeklyHours,
            command.TotalHours);

        await _curriculumRepository.AddAsync(entry, ct);
        return CreatedAtAction(nameof(GetAll), new { seriesId }, entry);
    }

    [HttpDelete("{subjectId:guid}")]
    public async Task<IActionResult> RemoveSubject(Guid seriesId, Guid subjectId, CancellationToken ct)
    {
        var seriesExists = await _seriesRepository.ExistsAsync(seriesId, ct);
        if (!seriesExists)
            return NotFound(new { error = "Series not found" });

        var entries = await _curriculumRepository.GetBySeriesAsync(seriesId, ct);
        var entry = entries.FirstOrDefault(e => e.SubjectId == subjectId);

        if (entry is null)
            return NotFound(new { error = "Subject not found in this series curriculum" });

        await _curriculumRepository.DeleteAsync(entry, ct);
        return NoContent();
    }
}