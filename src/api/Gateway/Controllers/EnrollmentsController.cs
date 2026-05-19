using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public EnrollmentsController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? classId, [FromQuery] int? schoolYear, [FromQuery] string? status)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var query = _db.Enrollments.Where(e => true); // filtered by student's org

        if (classId.HasValue) query = query.Where(e => e.ClassId == classId.Value);
        if (schoolYear.HasValue) query = query.Where(e => e.SchoolYear == schoolYear.Value);
        if (status != null) query = query.Where(e => e.Status == status);

        var enrollments = await query
            .Join(_db.Students, e => e.StudentId, s => s.Id, (e, s) => new { e, s })
            .Join(_db.Users, es => es.s.UserId, u => u.Id, (es, u) => new { es.e, es.s, u })
            .Where(x => x.s.OrganizationId == oid)
            .Join(_db.Classes, x => x.e.ClassId, c => c.Id, (x, c) => new
            {
                x.e.Id,
                studentId = x.s.Id,
                studentName = x.u.Name,
                enrollmentCode = x.s.Enrollment,
                classId = c.Id,
                className = c.Name,
                x.e.SchoolYear,
                x.e.Status,
                x.e.EnrollmentDate
            })
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        return Ok(enrollments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var enrollment = await _db.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound(new { message = "Matrícula não encontrada" });

        var student = await _db.Students.FindAsync(enrollment.StudentId);
        var classEntity = await _db.Classes.FindAsync(enrollment.ClassId);

        return Ok(new
        {
            enrollment.Id,
            enrollment.StudentId,
            enrollment.ClassId,
            className = classEntity?.Name,
            enrollment.SchoolYear,
            enrollment.Status,
            enrollment.EnrollmentDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        // Validate student exists
        if (!await _db.Students.AnyAsync(s => s.Id == req.StudentId))
            return BadRequest(new { message = "Aluno não encontrado" });

        // Validate class exists
        if (!await _db.Classes.AnyAsync(c => c.Id == req.ClassId))
            return BadRequest(new { message = "Turma não encontrada" });

        // Check for duplicate enrollment
        if (await _db.Enrollments.AnyAsync(e => e.StudentId == req.StudentId && e.ClassId == req.ClassId && e.SchoolYear == req.SchoolYear))
            return BadRequest(new { message = "Aluno já matriculado nesta turma/ano" });

        var enrollment = new Enrollment
        {
            StudentId = req.StudentId,
            ClassId = req.ClassId,
            SchoolYear = req.SchoolYear,
            Status = req.Status ?? "active"
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        return Ok(new { enrollment.Id, enrollment.Status });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnrollmentReq req)
    {
        var enrollment = await _db.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound(new { message = "Matrícula não encontrada" });

        if (req.ClassId.HasValue) enrollment.ClassId = req.ClassId.Value;
        if (req.Status != null) enrollment.Status = req.Status;

        await _db.SaveChangesAsync();
        return Ok(new { enrollment.Id, enrollment.Status });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var enrollment = await _db.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound(new { message = "Matrícula não encontrada" });

        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateEnrollmentReq(Guid StudentId, Guid ClassId, int SchoolYear, string? Status);
public record UpdateEnrollmentReq(Guid? ClassId, string? Status);
