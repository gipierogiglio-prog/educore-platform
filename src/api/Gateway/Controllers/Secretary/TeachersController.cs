using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly AppDbContext _db;
    public TeachersController(AppDbContext db) => _db = db;
    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        return Ok(await _db.Teachers.Where(t => t.OrganizationId == oid && t.Active)
            .Join(_db.Users, t => t.UserId, u => u.Id, (t, u) => new { t.Id, u.Name, u.Email, t.Specialization, t.HireDate })
            .OrderBy(x => x.Name).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TcrReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });
        if (await _db.Users.AnyAsync(u => u.Email == req.Email)) return BadRequest(new { message = "Email já existe" });
        var user = new User { Name = req.Name, Email = req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), Role = "teacher", OrganizationId = oid };
        _db.Users.Add(user); await _db.SaveChangesAsync();
        _db.Teachers.Add(new Teacher { UserId = user.Id, Specialization = req.Specialization, OrganizationId = oid.Value });
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Name, user.Email, req.Specialization });
    }
}
public record TcrReq(string Name, string Email, string Password, string? Specialization);
