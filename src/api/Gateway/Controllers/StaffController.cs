using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _db;
    public StaffController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var query = _db.Teachers.Where(t => t.OrganizationId == oid);
        if (activeOnly == true)
            query = query.Where(t => t.Active);

        return Ok(await query
            .Join(_db.Users, t => t.UserId, u => u.Id, (t, u) => new
            {
                t.Id,
                userId = u.Id,
                u.Name,
                u.Email,
                u.Phone,
                u.Role,
                t.Specialization,
                t.HireDate,
                t.Active
            })
            .OrderBy(s => s.Name)
            .ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var teacher = await _db.Teachers
            .Where(t => t.Id == id && t.OrganizationId == oid)
            .Join(_db.Users, t => t.UserId, u => u.Id, (t, u) => new
            {
                t.Id,
                userId = u.Id,
                u.Name,
                u.Email,
                u.Phone,
                u.Role,
                t.Specialization,
                t.HireDate,
                t.Active
            })
            .FirstOrDefaultAsync();

        if (teacher == null) return NotFound(new { message = "Funcionário não encontrado" });
        return Ok(teacher);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        // Create user first
        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password ?? "123456"),
            Role = "teacher",
            Phone = req.Phone,
            OrganizationId = oid.Value
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create teacher record
        var teacher = new Teacher
        {
            UserId = user.Id,
            Specialization = req.Specialization,
            OrganizationId = oid.Value
        };
        _db.Teachers.Add(teacher);
        await _db.SaveChangesAsync();

        return Ok(new { teacher.Id, user.Name, user.Email });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == oid);
        if (teacher == null) return NotFound(new { message = "Funcionário não encontrado" });

        if (req.Specialization != null) teacher.Specialization = req.Specialization;

        // Update user info if provided
        if (req.Name != null || req.Email != null || req.Phone != null)
        {
            var user = await _db.Users.FindAsync(teacher.UserId);
            if (user != null)
            {
                if (req.Name != null) user.Name = req.Name;
                if (req.Email != null) user.Email = req.Email;
                if (req.Phone != null) user.Phone = req.Phone;
                user.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { teacher.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == oid);
        if (teacher == null) return NotFound(new { message = "Funcionário não encontrado" });

        teacher.Active = false;

        // Also deactivate the user
        var user = await _db.Users.FindAsync(teacher.UserId);
        if (user != null)
        {
            user.Active = false;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateStaffReq(string Name, string Email, string? Password, string? Phone, string? Specialization);
public record UpdateStaffReq(string? Name, string? Email, string? Phone, string? Specialization);
