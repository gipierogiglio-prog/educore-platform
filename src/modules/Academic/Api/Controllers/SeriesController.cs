using Giglio.EduCore.Academic.Application.Commands.Series;
using Giglio.EduCore.Academic.Domain.Entities;
using Giglio.EduCore.Academic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giglio.EduCore.Academic.Api.Controllers;

[ApiController]
[Route("api/academic/series")]
public class SeriesController : ControllerBase
{
    private readonly ISeriesRepository _repository;

    public SeriesController(ISeriesRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var series = await _repository.GetAllAsync(activeOnly, ct);
        return Ok(series);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var series = await _repository.GetByIdAsync(id, ct);
        if (series is null)
            return NotFound(new { error = "Series not found" });
        return Ok(series);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSeriesCommand command, CancellationToken ct)
    {
        var validator = new CreateSeriesValidator();
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        var series = new Series(
            command.Name,
            command.CourseId,
            command.AcademicYear,
            command.TotalHours);

        await _repository.AddAsync(series, ct);
        return CreatedAtAction(nameof(GetById), new { id = series.Id }, series);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeriesCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Id mismatch" });

        var series = await _repository.GetByIdAsync(id, ct);
        if (series is null)
            return NotFound(new { error = "Series not found" });

        if (command.Name != null)
            series.SetName(command.Name);

        if (command.CourseId.HasValue)
            series.SetCourse(command.CourseId.Value);

        if (command.AcademicYear.HasValue)
            series.SetAcademicYear(command.AcademicYear.Value);

        if (command.TotalHours.HasValue)
            series.SetTotalHours(command.TotalHours);

        await _repository.UpdateAsync(series, ct);
        return Ok(series);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var series = await _repository.GetByIdAsync(id, ct);
        if (series is null)
            return NotFound(new { error = "Series not found" });

        series.Deactivate();
        await _repository.UpdateAsync(series, ct);
        return NoContent();
    }
}