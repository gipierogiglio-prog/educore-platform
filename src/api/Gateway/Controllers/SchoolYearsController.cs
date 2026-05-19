using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/schoolyears")]
[Authorize]
public class SchoolYearsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SchoolYearsController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.SchoolYears.OrderByDescending(s => s.Year).Select(s => new
        {
            s.Id,
            s.Year,
            s.StartDate,
            s.EndDate,
            s.Description,
            s.Status
        }).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sy = await _db.SchoolYears.FindAsync(id);
        if (sy == null) return NotFound(new { message = "Ano letivo não encontrado" });

        return Ok(new
        {
            sy.Id,
            sy.Year,
            sy.StartDate,
            sy.EndDate,
            sy.Description,
            sy.Status
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSchoolYearReq req)
    {
        if (await _db.SchoolYears.AnyAsync(s => s.Year == req.Year))
            return BadRequest(new { message = "Ano letivo já cadastrado" });

        var sy = new SchoolYear
        {
            Year = req.Year,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Description = req.Description,
            Status = req.Status ?? "planned"
        };

        _db.SchoolYears.Add(sy);
        await _db.SaveChangesAsync();

        return Ok(new { sy.Id, sy.Year, sy.Status });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolYearReq req)
    {
        var sy = await _db.SchoolYears.FindAsync(id);
        if (sy == null) return NotFound(new { message = "Ano letivo não encontrado" });

        if (req.StartDate.HasValue) sy.StartDate = req.StartDate.Value;
        if (req.EndDate.HasValue) sy.EndDate = req.EndDate.Value;
        if (req.Description != null) sy.Description = req.Description;
        if (req.Status != null) sy.Status = req.Status;

        await _db.SaveChangesAsync();
        return Ok(new { sy.Id, sy.Year, sy.Status });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sy = await _db.SchoolYears.FindAsync(id);
        if (sy == null) return NotFound(new { message = "Ano letivo não encontrado" });

        _db.SchoolYears.Remove(sy);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateSchoolYearReq(int Year, DateTime StartDate, DateTime EndDate, string? Description, string? Status);
public record UpdateSchoolYearReq(DateTime? StartDate, DateTime? EndDate, string? Description, string? Status);
