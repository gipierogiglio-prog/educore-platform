using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/academic/assign-teacher")]
[Authorize]
public class AssignTeacherController : ControllerBase
{
    private readonly AppDbContext _db;
    public AssignTeacherController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAllAssignments()
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var assignments = await _db.TeacherSubjects
            .Join(_db.Teachers, ts => ts.TeacherId, t => t.Id, (ts, t) => new { ts, t })
            .Where(x => x.t.OrganizationId == oid)
            .Join(_db.Users, x => x.t.UserId, u => u.Id, (x, u) => new { x.ts, teacherName = u.Name })
            .Join(_db.Subjects, x => x.ts.SubjectId, s => s.Id, (x, s) => new { x.ts, x.teacherName, subjectName = s.Name })
            .Join(_db.Classes, x => x.ts.ClassId, c => c.Id, (x, c) => new
            {
                x.ts.Id,
                teacherId = x.ts.TeacherId,
                teacherName = x.teacherName,
                subjectId = x.ts.SubjectId,
                subjectName = x.subjectName,
                classId = c.Id,
                className = c.Name,
                classYear = c.Year
            })
            .OrderBy(a => a.className).ThenBy(a => a.subjectName)
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var assignment = await _db.TeacherSubjects.FindAsync(id);
        if (assignment == null) return NotFound(new { message = "Vínculo não encontrado" });

        return Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignTeacherReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        // Validate teacher
        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == req.TeacherId && t.OrganizationId == oid);
        if (teacher == null) return BadRequest(new { message = "Professor não encontrado" });

        // Validate subject
        if (!await _db.Subjects.AnyAsync(s => s.Id == req.SubjectId && s.OrganizationId == oid))
            return BadRequest(new { message = "Disciplina não encontrada" });

        // Validate class
        if (!await _db.Classes.AnyAsync(c => c.Id == req.ClassId && c.OrganizationId == oid))
            return BadRequest(new { message = "Turma não encontrada" });

        // Check for duplicate
        if (await _db.TeacherSubjects.AnyAsync(ts =>
            ts.TeacherId == req.TeacherId && ts.SubjectId == req.SubjectId && ts.ClassId == req.ClassId))
            return BadRequest(new { message = "Professor já vinculado a esta disciplina/turma" });

        var ts = new TeacherSubject
        {
            TeacherId = req.TeacherId,
            SubjectId = req.SubjectId,
            ClassId = req.ClassId
        };

        _db.TeacherSubjects.Add(ts);
        await _db.SaveChangesAsync();

        return Ok(new { ts.Id, message = "Professor vinculado com sucesso" });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignTeacherReq req)
    {
        var assignment = await _db.TeacherSubjects.FindAsync(id);
        if (assignment == null) return NotFound(new { message = "Vínculo não encontrado" });

        if (req.TeacherId.HasValue) assignment.TeacherId = req.TeacherId.Value;
        if (req.SubjectId.HasValue) assignment.SubjectId = req.SubjectId.Value;
        if (req.ClassId.HasValue) assignment.ClassId = req.ClassId.Value;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Vínculo atualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var assignment = await _db.TeacherSubjects.FindAsync(id);
        if (assignment == null) return NotFound(new { message = "Vínculo não encontrado" });

        _db.TeacherSubjects.Remove(assignment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record AssignTeacherReq(Guid TeacherId, Guid SubjectId, Guid ClassId);
public record UpdateAssignTeacherReq(Guid? TeacherId, Guid? SubjectId, Guid? ClassId);
