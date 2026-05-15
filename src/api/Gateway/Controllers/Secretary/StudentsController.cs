using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StudentsController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        var data = await _db.Students.Where(s => s.OrganizationId == oid && s.Active)
            .Join(_db.Users, s => s.UserId, u => u.Id, (s, u) => new { s.Id, u.Name, u.Email, s.Enrollment, s.ClassId, s.EnrollmentDate })
            .OrderBy(x => x.Name).ToListAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StuReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        if (await _db.Users.AnyAsync(u => u.Email == req.Email)) return BadRequest(new { message = "Email já existe" });
        var user = new User { Name = req.Name, Email = req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), Role = "student", OrganizationId = oid };
        _db.Users.Add(user); await _db.SaveChangesAsync();
        var enrollment = $"STU{DateTime.UtcNow:yyyy}{await _db.Students.CountAsync(s => s.OrganizationId == oid) + 1:D4}";
        _db.Students.Add(new Student { UserId = user.Id, Enrollment = enrollment, ClassId = req.ClassId, OrganizationId = oid.Value });
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Name, user.Email, enrollment, req.ClassId });
    }

    [HttpPatch("{id}/class")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] ClsReq req)
    {
        var oid = OrgId; 
        var s = await _db.Students.FirstOrDefaultAsync(x => x.Id == id && x.OrganizationId == oid);
        if (s == null) return NotFound();
        s.ClassId = req.ClassId; await _db.SaveChangesAsync();
        return Ok(new { message = "Turma atualizada" });
    }
}
public record StuReq(string Name, string Email, string Password, Guid? ClassId);
public record ClsReq(Guid? ClassId);
