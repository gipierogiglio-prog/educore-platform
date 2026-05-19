using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/grading")]
[Authorize]
public class GradingController : ControllerBase
{
    private readonly AppDbContext _db;
    public GradingController(AppDbContext db) => _db = db;
    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    // === CRUD Regras de Cálculo ===
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var rules = await _db.GradeRules.Where(r => r.OrganizationId == oid)
            .Include(r => r.Components.OrderBy(c => c.Order))
            .Select(r => new { r.Id, r.Name, r.Description, r.Active, r.CreatedAt, Components = r.Components.Select(c => new { c.Id, c.Name, c.Type, c.Weight, c.MaxValue, c.Order, c.HasRecovery }) })
            .ToListAsync();
        return Ok(rules);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] CreateRuleReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var rule = new GradeRule { Name = req.Name, Description = req.Description, OrganizationId = oid.Value };
        _db.GradeRules.Add(rule);
        await _db.SaveChangesAsync();

        if (req.Components?.Any() == true)
        {
            for (int i = 0; i < req.Components.Count; i++)
            {
                var c = req.Components[i];
                rule.Components.Add(new GradeRuleComponent
                {
                    GradeRuleId = rule.Id, Name = c.Name, Type = c.Type,
                    Weight = c.Weight, MaxValue = c.MaxValue, Order = i, HasRecovery = c.HasRecovery
                });
            }
            await _db.SaveChangesAsync();
        }
        return Ok(new { rule.Id, rule.Name, Components = rule.Components.Count });
    }

    [HttpPut("rules/{id}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] CreateRuleReq req)
    {
        var oid = OrgId;
        var rule = await _db.GradeRules.Include(r => r.Components)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == oid);
        if (rule == null) return NotFound();

        rule.Name = req.Name;
        rule.Description = req.Description;
        _db.GradeRuleComponents.RemoveRange(rule.Components);
        rule.Components.Clear();

        if (req.Components?.Any() == true)
        {
            for (int i = 0; i < req.Components.Count; i++)
            {
                var c = req.Components[i];
                rule.Components.Add(new GradeRuleComponent { GradeRuleId = rule.Id, Name = c.Name, Type = c.Type, Weight = c.Weight, MaxValue = c.MaxValue, Order = i, HasRecovery = c.HasRecovery });
            }
        }
        await _db.SaveChangesAsync();
        return Ok(new { rule.Id, rule.Name });
    }

    [HttpDelete("rules/{id}")]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        var oid = OrgId;
        var rule = await _db.GradeRules.FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == oid);
        if (rule == null) return NotFound();
        _db.GradeRules.Remove(rule);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Regra removida" });
    }

    // === Lançar notas por componente ===
    [HttpPost("entries")]
    public async Task<IActionResult> SubmitEntries([FromBody] SubmitEntryReq req)
    {
        var entries = req.Entries.Select(e => new GradeEntry
        {
            StudentId = e.StudentId, GradeRuleComponentId = req.ComponentId,
            Bimester = req.Bimester, SchoolYear = req.SchoolYear ?? DateTime.UtcNow.Year,
            Value = e.Value, RecoveryValue = e.RecoveryValue, OrganizationId = Guid.Empty
        }).ToList();
        _db.GradeEntries.AddRange(entries);
        await _db.SaveChangesAsync();
        return Ok(new { count = entries.Count });
    }

    // === Calcular média ===
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] CalculateReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var rule = await _db.GradeRules.Include(r => r.Components)
            .FirstOrDefaultAsync(r => r.Id == req.RuleId);
        if (rule == null) return NotFound(new { message = "Regra não encontrada" });

        var results = new List<object>();
        foreach (var studentId in req.StudentIds)
        {
            decimal totalWeight = 0, totalValue = 0;
            foreach (var comp in rule.Components)
            {
                var entry = await _db.GradeEntries
                    .Where(e => e.StudentId == studentId && e.GradeRuleComponentId == comp.Id
                        && e.Bimester == req.Bimester && e.SchoolYear == req.SchoolYear)
                    .FirstOrDefaultAsync();

                if (entry?.Value != null)
                {
                    var value = entry.RecoveryValue ?? entry.Value.Value;
                    totalValue += value * comp.Weight;
                    totalWeight += comp.Weight;
                }
            }

            var finalValue = totalWeight > 0 ? Math.Round(totalValue / totalWeight, 2) : 0;
            results.Add(new { studentId, finalValue, status = finalValue >= (req.PassingGrade ?? 6) ? "approved" : "recovery" });
        }
        return Ok(results);
    }

    // === BOLETIM CONSOLIDADO do aluno ===
    [HttpGet("students/{studentId}/report")]
    public async Task<IActionResult> GetStudentReport(Guid studentId, [FromQuery] int? year)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var y = year ?? DateTime.UtcNow.Year;

        // Student data
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.OrganizationId == oid);
        if (student == null)
            return NotFound(new { message = "Aluno não encontrado" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == student.UserId);

        // Class data
        var classData = await _db.Classes
            .FirstOrDefaultAsync(c => c.Id == student.ClassId);

        // Enrollment
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.SchoolYear == y);

        // Subjects for this class
        var subjectIds = await _db.TeacherSubjects
            .Where(ts => ts.ClassId == student.ClassId)
            .Select(ts => ts.SubjectId)
            .Distinct()
            .ToListAsync();

        var subjects = await _db.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .OrderBy(s => s.Name)
            .ToListAsync();

        // Grades per subject per bimester (simple grades table)
        var allGrades = await _db.Grades
            .Where(g => g.StudentId == studentId && g.SchoolYear == y)
            .ToListAsync();

        // Grading rules entries
        var rules = await _db.GradeRules
            .Where(r => r.OrganizationId == oid && r.Active)
            .Include(r => r.Components.OrderBy(c => c.Order))
            .ToListAsync();

        var defaultRule = rules.FirstOrDefault();

        var allEntries = defaultRule != null
            ? await _db.GradeEntries
                .Where(e => e.StudentId == studentId && e.SchoolYear == y)
                .ToListAsync()
            : new List<GradeEntry>();

        // Build subject report per bimester
        var subjectsReport = new List<object>();
        decimal totalFinalAverage = 0;
        int subjectCount = 0;

        foreach (var subject in subjects)
        {
            var bimesterGrades = new List<object>();
            decimal subjectFinalSum = 0;
            int bimestersWithGrades = 0;

            for (int b = 1; b <= 4; b++)
            {
                // Simple grade for this subject/bimester
                var simpleGrade = allGrades
                    .FirstOrDefault(g => g.SubjectId == subject.Id && g.Bimester == b);

                // Grading entries for this subject (via shared component lookup)
                decimal? calculatedAverage = null;
                if (defaultRule != null && defaultRule.Components.Any())
                {
                    decimal totalWeight = 0, totalValue = 0;
                    foreach (var comp in defaultRule.Components)
                    {
                        var entry = allEntries
                            .FirstOrDefault(e => e.GradeRuleComponentId == comp.Id && e.Bimester == b);

                        if (entry?.Value != null)
                        {
                            var val = entry.RecoveryValue ?? entry.Value.Value;
                            totalValue += val * comp.Weight;
                            totalWeight += comp.Weight;
                        }
                    }
                    if (totalWeight > 0)
                        calculatedAverage = Math.Round(totalValue / totalWeight, 2);
                }

                var bimesterValue = calculatedAverage ?? simpleGrade?.Value;
                var bimesterRecovery = simpleGrade?.RecoveryValue;

                bimesterGrades.Add(new
                {
                    bimester = b,
                    value = bimesterValue,
                    recoveryValue = bimesterRecovery,
                    hasRecoveryGrade = simpleGrade?.RecoveryValue != null
                });

                if (bimesterValue.HasValue)
                {
                    subjectFinalSum += bimesterValue.Value;
                    bimestersWithGrades++;
                }
            }

            var finalAverage = bimestersWithGrades > 0
                ? Math.Round(subjectFinalSum / bimestersWithGrades, 2)
                : (decimal?)null;

            var passingGrade = 6m;
            var status = !finalAverage.HasValue ? "pending"
                : finalAverage >= passingGrade ? "approved"
                : finalAverage >= 4m ? "recovery" : "reproved";

            subjectsReport.Add(new
            {
                subjectId = subject.Id,
                subjectName = subject.Name,
                subjectCode = subject.Code,
                workload = subject.Workload,
                finalAverage,
                passingGrade,
                status,
                bimesters = bimesterGrades
            });

            if (finalAverage.HasValue)
            {
                totalFinalAverage += finalAverage.Value;
                subjectCount++;
            }
        }

        var overallAverage = subjectCount > 0
            ? Math.Round(totalFinalAverage / subjectCount, 2)
            : (decimal?)null;

        var overallStatus = !overallAverage.HasValue ? "pending"
            : overallAverage >= 6m ? "approved"
            : overallAverage >= 4m ? "recovery" : "reproved";

        return Ok(new
        {
            student = new
            {
                id = student.Id,
                enrollment = student.Enrollment,
                name = user?.Name ?? "",
                email = user?.Email ?? ""
            },
            classInfo = classData == null ? null : new
            {
                id = classData.Id,
                name = classData.Name,
                shift = classData.Shift,
                year = classData.Year
            },
            schoolYear = y,
            enrollmentStatus = enrollment?.Status ?? "unknown",
            gradingRule = defaultRule == null ? null : new { defaultRule.Id, defaultRule.Name },
            subjects = subjectsReport,
            overall = new
            {
                totalSubjects = subjects.Count,
                approvedCount = subjectsReport.Count(s =>
                {
                    var x = (dynamic)s;
                    return x.status == "approved";
                }),
                recoveryCount = subjectsReport.Count(s =>
                {
                    var x = (dynamic)s;
                    return x.status == "recovery";
                }),
                reprovedCount = subjectsReport.Count(s =>
                {
                    var x = (dynamic)s;
                    return x.status == "reproved";
                }),
                average = overallAverage,
                status = overallStatus
            }
        });
    }

    // === Obter notas de um aluno ===
    [HttpGet("students/{studentId}/grades")]
    public async Task<IActionResult> GetStudentGrades(Guid studentId, [FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var entries = await _db.GradeEntries
            .Where(e => e.StudentId == studentId && e.SchoolYear == y)
            .Join(_db.GradeRuleComponents, e => e.GradeRuleComponentId, c => c.Id, (e, c) => new { e.Id, ComponentName = c.Name, c.Type, c.Weight, c.MaxValue, e.Bimester, e.Value, e.RecoveryValue })
            .OrderBy(x => x.Bimester).ThenBy(x => x.ComponentName).ToListAsync();
        return Ok(entries);
    }
}

public record CreateRuleReq(string Name, string? Description, List<CompReq>? Components);
public record CompReq(string Name, string Type, decimal Weight, int MaxValue, bool HasRecovery);
public record SubmitEntryReq(Guid ComponentId, int Bimester, int? SchoolYear, List<EntryItem> Entries);
public record EntryItem(Guid StudentId, decimal? Value, decimal? RecoveryValue);
public record CalculateReq(Guid RuleId, List<Guid> StudentIds, int Bimester, int SchoolYear, decimal? PassingGrade);
