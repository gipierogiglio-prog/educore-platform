using System.Security.Claims;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "org_admin,coordinator")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    /// <summary>
    /// Main report endpoint returning general institution statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var totalStudents = await _db.Students.CountAsync(s => s.OrganizationId == oid && s.Active);
        var totalTeachers = await _db.Teachers.CountAsync(t => t.OrganizationId == oid && t.Active);
        var totalClasses = await _db.Classes.CountAsync(c => c.OrganizationId == oid && c.Active);
        var totalUsers = await _db.Users.CountAsync(u => u.OrganizationId == oid && u.Active);

        // Students per class
        var studentsByClass = await _db.Students
            .Where(s => s.OrganizationId == oid && s.ClassId != null && s.Active)
            .GroupBy(s => s.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToListAsync();

        var classIds = studentsByClass.Select(s => s.ClassId!.Value).ToList();
        var classNames = await _db.Classes
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var byClass = studentsByClass.Select(s => new
        {
            className = classNames.GetValueOrDefault(s.ClassId!.Value, "Sem turma"),
            count = s.Count
        }).ToList();

        // Active enrollments count
        var activeEnrollments = await _db.Enrollments
            .CountAsync(e => e.Status == "active");

        return Ok(new
        {
            totalStudents,
            totalTeachers,
            totalClasses,
            totalUsers,
            activeEnrollments,
            studentsByClass = byClass
        });
    }

    /// <summary>
    /// Students report with enrollment details
    /// </summary>
    [HttpGet("students")]
    public async Task<IActionResult> StudentsReport([FromQuery] int? schoolYear)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var year = schoolYear ?? DateTime.UtcNow.Year;

        var data = await _db.Students
            .Where(s => s.OrganizationId == oid && s.Active)
            .Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u })
            .GroupJoin(
                _db.Enrollments.Where(e => e.SchoolYear == year),
                x => x.s.Id,
                e => e.StudentId,
                (x, enrollments) => new { x.s, x.u, Enrollments = enrollments })
            .SelectMany(
                x => x.Enrollments.DefaultIfEmpty(),
                (x, e) => new
                {
                    studentId = x.s.Id,
                    studentName = x.u.Name,
                    enrollmentCode = x.s.Enrollment,
                    x.s.EnrollmentDate,
                    classId = e != null ? e.ClassId : (Guid?)null,
                    enrollmentStatus = e != null ? e.Status : "not_enrolled",
                    schoolYear = e != null ? e.SchoolYear : (int?)null
                })
            .ToListAsync();

        return Ok(data);
    }

    /// <summary>
    /// Attendance report summary
    /// </summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> AttendanceReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var stats = await _db.Attendances
            .Where(a => a.Date >= start && a.Date <= end)
            .GroupBy(a => a.ClassId)
            .Select(g => new
            {
                classId = g.Key,
                total = g.Count(),
                present = g.Count(a => a.Present),
                absent = g.Count(a => !a.Present)
            })
            .ToListAsync();

        var classIds2 = stats.Select(s => s.classId).ToList();
        var classNames2 = await _db.Classes
            .Where(c => classIds2.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        return Ok(stats.Select(s => new
        {
            className = classNames2.GetValueOrDefault(s.classId, "N/A"),
            s.total,
            s.present,
            s.absent,
            frequencyPercent = s.total > 0 ? Math.Round((double)s.present / s.total * 100, 1) : 0
        }).ToList());
    }

    /// <summary>
    /// General school year report with class summaries
    /// </summary>
    [HttpGet("year/{year:int}")]
    public async Task<IActionResult> YearReport(int year)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var schoolYear = await _db.SchoolYears.FirstOrDefaultAsync(s => s.Year == year);
        if (schoolYear == null)
            return NotFound(new { message = "Ano letivo não encontrado" });

        var classes = await _db.Classes
            .Where(c => c.OrganizationId == oid && c.Year == year)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Shift,
                c.Room,
                studentCount = _db.Enrollments.Count(e => e.ClassId == c.Id && e.SchoolYear == year)
            })
            .ToListAsync();

        return Ok(new
        {
            schoolYear = new { schoolYear.Id, schoolYear.Year, schoolYear.Status },
            classes,
            totalClasses = classes.Count,
            totalStudents = classes.Sum(c => c.studentCount)
        });
    }
}
