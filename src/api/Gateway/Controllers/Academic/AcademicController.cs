using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/academic")]
[Authorize]
public class AcademicController : ControllerBase
{
    private readonly AppDbContext _db;
    public AcademicController(AppDbContext db) => _db = db;
    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        return Ok(await _db.Classes.Where(c => c.OrganizationId == oid)
            .Select(c => new { c.Id, c.Name, c.Shift, c.Year, c.Room, c.Active })
            .OrderBy(c => c.Name).ToListAsync());
    }

    [HttpPost("classes")]
    public async Task<IActionResult> CreateClass([FromBody] ClsReq2 req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var c = new Class { Name = req.Name, Shift = req.Shift, Year = req.Year ?? DateTime.UtcNow.Year, Room = req.Room, OrganizationId = oid.Value };
        _db.Classes.Add(c); await _db.SaveChangesAsync();
        return Ok(new { c.Id, c.Name });
    }

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        return Ok(await _db.Subjects.Where(s => s.OrganizationId == oid && s.Active).OrderBy(s => s.Name).ToListAsync());
    }

    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject([FromBody] SubReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        if (await _db.Subjects.AnyAsync(s => s.Code == req.Code && s.OrganizationId == oid))
            return BadRequest(new { message = "Código já existe" });
        var s = new Subject { Name = req.Name, Code = req.Code, Workload = req.Workload, OrganizationId = oid.Value };
        _db.Subjects.Add(s); await _db.SaveChangesAsync();
        return Ok(new { s.Id, s.Name, s.Code });
    }

    [HttpPost("grades/batch")]
    public async Task<IActionResult> SubmitGrades([FromBody] GrdReq req)
    {
        var grades = req.Grades.Select(g => new Grade { StudentId = g.S, SubjectId = req.SubjectId, Bimester = req.Bimester, Value = g.V, RecoveryValue = g.R, SchoolYear = req.Year ?? DateTime.UtcNow.Year, OrganizationId = Guid.Empty });
        _db.Grades.AddRange(grades); await _db.SaveChangesAsync();
        return Ok(new { count = grades.Count() });
    }

    [HttpPost("attendance/batch")]
    public async Task<IActionResult> SubmitAttendance([FromBody] AttReq req)
    {
        var att = req.Items.Select(a => new Attendance { StudentId = a.S, ClassId = req.ClassId, SubjectId = req.SubjectId, Date = req.Date, Present = a.P, Justification = a.J, OrganizationId = Guid.Empty });
        _db.Attendances.AddRange(att); await _db.SaveChangesAsync();
        return Ok(new { count = att.Count() });
    }
}
public record ClsReq2(string Name, string Shift, int? Year, string? Room);
public record SubReq(string Name, string Code, int Workload);
public record GrdReq(Guid SubjectId, int Bimester, int? Year, List<GrdItem> Grades);
public record GrdItem(Guid S, decimal V, decimal? R);
public record AttReq(Guid ClassId, Guid? SubjectId, DateTime Date, List<AttItem> Items);
public record AttItem(Guid S, bool P, string? J);
