using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    private Guid? UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;
    private string? Role => User.FindFirstValue(ClaimTypes.Role);

    /// <summary>
    /// Main organization dashboard (admin/coordinator view)
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

        var recentStudents = await _db.Students
            .Where(s => s.OrganizationId == oid)
            .OrderByDescending(s => s.EnrollmentDate)
            .Take(5)
            .Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s.Id, u.Name, s.Enrollment, s.EnrollmentDate })
            .ToListAsync();

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
            name = classNames.GetValueOrDefault(s.ClassId!.Value, "Sem turma"),
            count = s.Count
        }).ToList();

        return Ok(new
        {
            totalStudents,
            totalTeachers,
            totalClasses,
            totalUsers,
            recentStudents,
            studentsByClass = byClass
        });
    }

    /// <summary>
    /// Teacher dashboard — their classes, subjects, grades, attendance
    /// </summary>
    [HttpGet("teacher")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> TeacherDashboard()
    {
        var uid = UserId;
        var oid = OrgId;
        if (uid == null || oid == null)
            return BadRequest(new { message = "Usuário ou organização não encontrada" });

        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == uid && t.OrganizationId == oid);
        if (teacher == null)
            return BadRequest(new { message = "Professor não encontrado" });

        // Teacher's subjects and classes
        var teacherSubjects = await _db.TeacherSubjects
            .Where(ts => ts.TeacherId == teacher.Id)
            .ToListAsync();

        var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).Distinct().ToList();
        var classIds = teacherSubjects.Select(ts => ts.ClassId).Distinct().ToList();
        var schoolYear = DateTime.UtcNow.Year;

        var subjects = await _db.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var classes = await _db.Classes
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var myClasses = teacherSubjects.Select(ts => new
        {
            subjectId = ts.SubjectId,
            subjectName = subjects.GetValueOrDefault(ts.SubjectId, "N/A"),
            classId = ts.ClassId,
            className = classes.GetValueOrDefault(ts.ClassId, "N/A")
        }).GroupBy(x => x.classId).Select(g => new
        {
            classId = g.Key,
            className = g.First().className,
            subjects = g.Select(x => new { subjectId = x.subjectId, subjectName = x.subjectName }).ToList()
        }).ToList();

        // Recent grades (last 10 entries)
        var studentsInClasses = await _db.Students
            .Where(s => classIds.Contains(s.ClassId ?? Guid.Empty) && s.OrganizationId == oid)
            .Select(s => s.Id)
            .ToListAsync();

        var recentGrades = await _db.Grades
            .Where(g => studentsInClasses.Contains(g.StudentId) && g.OrganizationId == oid && g.SchoolYear == schoolYear)
            .OrderByDescending(g => g.Id)
            .Take(20)
            .Join(_db.Users, g => g.StudentId, u => u.Id, (g, u) => new
            {
                studentId = g.StudentId,
                studentName = u.Name,
                subjectId = g.SubjectId,
                bimester = g.Bimester,
                value = g.Value,
                recoveryValue = g.RecoveryValue
            })
            .ToListAsync();

        // Attendance stats per class (last 30 days)
        var last30 = DateTime.UtcNow.AddDays(-30);
        var attendanceStats = await _db.Attendances
            .Where(a => classIds.Contains(a.ClassId) && a.Date >= last30)
            .GroupBy(a => a.ClassId)
            .Select(g => new
            {
                classId = g.Key,
                total = g.Count(),
                present = g.Count(a => a.Present),
                absent = g.Count(a => !a.Present)
            })
            .ToListAsync();

        var attendanceWithNames = attendanceStats.Select(a => new
        {
            className = classes.GetValueOrDefault(a.classId, "N/A"),
            a.total, a.present, a.absent
        }).ToList();

        // Total students per class
        var studentCountByClass = await _db.Students
            .Where(s => classIds.Contains(s.ClassId ?? Guid.Empty) && s.Active && s.OrganizationId == oid)
            .GroupBy(s => s.ClassId)
            .Select(g => new { classId = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            teacherId = teacher.Id,
            specialization = teacher.Specialization,
            hireDate = teacher.HireDate,
            totalClasses = myClasses.Count,
            totalSubjects = subjectIds.Count,
            classes = myClasses,
            studentCountByClass = studentCountByClass.Select(s => new
            {
                className = classes.GetValueOrDefault(s.classId ?? Guid.Empty, "N/A"),
                s.count
            }).ToList(),
            recentGrades,
            attendanceLast30Days = attendanceWithNames
        });
    }

    /// <summary>
    /// Guardian dashboard — children, grades, attendance, occurrences
    /// </summary>
    [HttpGet("guardian")]
    [Authorize(Roles = "guardian")]
    public async Task<IActionResult> GuardianDashboard()
    {
        var uid = UserId;
        var oid = OrgId;
        if (uid == null || oid == null)
            return BadRequest(new { message = "Usuário ou organização não encontrada" });

        var guardian = await _db.Guardians.FirstOrDefaultAsync(g => g.UserId == uid);
        if (guardian == null)
            return BadRequest(new { message = "Responsável não encontrado" });

        var schoolYear = DateTime.UtcNow.Year;

        // Get children (students linked to this guardian)
        var children = await _db.Students
            .Where(s => s.GuardianId == guardian.Id && s.Active)
            .Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s.Id, u.Name, u.Email, s.Enrollment, s.ClassId })
            .ToListAsync();

        var classIds = children.Where(c => c.ClassId != null).Select(c => c.ClassId!.Value).Distinct().ToList();
        var classes = await _db.Classes
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var studentIds = children.Select(c => c.Id).ToList();
        var studentUserIds = await _db.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.UserId);

        // Grades for all children
        var grades = await _db.Grades
            .Where(g => studentUserIds.Values.Contains(g.StudentId) && g.SchoolYear == schoolYear)
            .Join(_db.Users, g => g.StudentId, u => u.Id, (g, u) => new { g, studentName = u.Name })
            .GroupBy(x => x.g.StudentId)
            .Select(g => new
            {
                studentId = g.Key,
                grades = g.Select(x => new
                {
                    subjectId = x.g.SubjectId,
                    bimester = x.g.Bimester,
                    value = x.g.Value,
                    recoveryValue = x.g.RecoveryValue
                }).ToList()
            })
            .ToListAsync();

        // Get subject names
        var subjectIds = grades.SelectMany(g => g.grades.Select(gg => gg.subjectId)).Distinct().ToList();
        var subjectNames = await _db.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        // Attendance for all children (current year)
        var attendance = await _db.Attendances
            .Where(a => studentIds.Contains(a.StudentId) && a.Date.Year == DateTime.UtcNow.Year)
            .GroupBy(a => a.StudentId)
            .Select(g => new
            {
                studentId = g.Key,
                total = g.Count(),
                present = g.Count(a => a.Present),
                absent = g.Count(a => !a.Present)
            })
            .ToListAsync();

        // Recent occurrences (if any — using attendance justifications as proxy)
        var recentOccurrences = await _db.Attendances
            .Where(a => studentIds.Contains(a.StudentId) && !a.Present && !string.IsNullOrEmpty(a.Justification))
            .OrderByDescending(a => a.Date)
            .Take(20)
            .Select(a => new
            {
                date = a.Date,
                justification = a.Justification
            })
            .ToListAsync();

        var childrenData = children.Select(child =>
        {
            var childGrades = grades.FirstOrDefault(g => g.studentId == child.UserId)?.grades ?? new List<dynamic>();
            var childAttendance = attendance.FirstOrDefault(a => a.studentId == child.Id);
            var childOccurrences = recentOccurrences.Count; // simplified

            return new
            {
                child.Id,
                child.Name,
                child.Email,
                child.Enrollment,
                className = classes.GetValueOrDefault(child.ClassId ?? Guid.Empty, "Sem turma"),
                grades = childGrades.Select(g => new
                {
                    subjectName = subjectNames.GetValueOrDefault(g.subjectId, "N/A"),
                    g.bimester,
                    g.value,
                    g.recoveryValue
                }).ToList(),
                attendance = childAttendance == null ? null : new
                {
                    total = childAttendance.total,
                    present = childAttendance.present,
                    absent = childAttendance.absent,
                    frequencyPercent = childAttendance.total > 0
                        ? Math.Round((double)childAttendance.present / childAttendance.total * 100, 1)
                        : 0
                }
            };
        }).ToList();

        return Ok(new
        {
            guardianId = guardian.Id,
            relationship = guardian.Relationship,
            children = childrenData
        });
    }
}
