using Giglio.EduCore.Academic.Domain.Entities;
using Giglio.EduCore.Academic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly AcademicDbContext _db;
    public CoursesController(AcademicDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.Courses.AsQueryable();
        if (activeOnly == true)
            query = query.Where(c => c.IsActive);

        return Ok(await query.OrderBy(c => c.Name).Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.TotalHours,
            c.IsActive,
            c.CreatedAt
        }).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound(new { message = "Curso não encontrado" });

        return Ok(new
        {
            course.Id,
            course.Name,
            course.Description,
            course.TotalHours,
            course.IsActive,
            course.CreatedAt,
            course.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseReq req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Nome é obrigatório" });

        var course = new Course(req.Name, req.TotalHours, req.Description);
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return Ok(new { course.Id, course.Name, course.TotalHours });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseReq req)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound(new { message = "Curso não encontrado" });

        if (req.Name != null) course.SetName(req.Name);
        if (req.TotalHours.HasValue) course.SetTotalHours(req.TotalHours.Value);
        if (req.Description != null) course.SetDescription(req.Description);

        await _db.SaveChangesAsync();
        return Ok(new { course.Id, course.Name, course.TotalHours });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound(new { message = "Curso não encontrado" });

        course.Deactivate();
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateCourseReq(string Name, int TotalHours, string? Description);
public record UpdateCourseReq(string? Name, int? TotalHours, string? Description);
