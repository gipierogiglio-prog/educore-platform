using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var query = _db.Users.Where(u => u.OrganizationId == oid);
        if (activeOnly == true)
            query = query.Where(u => u.Active);

        return Ok(await query.OrderBy(u => u.Name).Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.Role,
            u.Phone,
            u.Active,
            u.CreatedAt
        }).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == oid);
        if (user == null) return NotFound(new { message = "Usuário não encontrado" });

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.Phone,
            user.Active,
            user.CreatedAt,
            user.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email && u.OrganizationId == oid))
            return BadRequest(new { message = "E-mail já cadastrado" });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password ?? "123456"),
            Role = req.Role ?? "teacher",
            Phone = req.Phone,
            OrganizationId = oid.Value
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Name, user.Email, user.Role });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == oid);
        if (user == null) return NotFound(new { message = "Usuário não encontrado" });

        if (req.Name != null) user.Name = req.Name;
        if (req.Email != null) user.Email = req.Email;
        if (req.Role != null) user.Role = req.Role;
        if (req.Phone != null) user.Phone = req.Phone;
        if (req.Password != null) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Name, user.Email, user.Role });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == oid);
        if (user == null) return NotFound(new { message = "Usuário não encontrado" });

        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateUserReq(string Name, string Email, string? Password, string? Role, string? Phone);
public record UpdateUserReq(string? Name, string? Email, string? Password, string? Role, string? Phone);
