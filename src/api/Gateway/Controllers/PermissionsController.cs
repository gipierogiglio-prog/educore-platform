using System.Security.Claims;
using Educore.Core.Entities;
using Educore.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Educore.Api.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(Roles = "org_admin")]
public class PermissionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PermissionsController(AppDbContext db) => _db = db;

    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    // ─── Permissions ───────────────────────────────────────

    [HttpGet("resources")]
    public async Task<IActionResult> GetAllPermissions()
    {
        return Ok(await _db.Permissions.OrderBy(p => p.Resource).ThenBy(p => p.Action)
            .Select(p => new { p.Id, p.Resource, p.Action, p.Name }).ToListAsync());
    }

    [HttpPost("resources")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionReq req)
    {
        if (await _db.Permissions.AnyAsync(p => p.Resource == req.Resource && p.Action == req.Action))
            return BadRequest(new { message = "Permissão já existe" });

        var perm = new Permission { Resource = req.Resource, Action = req.Action, Name = req.Name };
        _db.Permissions.Add(perm);
        await _db.SaveChangesAsync();
        return Ok(new { perm.Id, perm.Resource, perm.Action });
    }

    [HttpDelete("resources/{id:guid}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var perm = await _db.Permissions.FindAsync(id);
        if (perm == null) return NotFound(new { message = "Permissão não encontrada" });

        _db.Permissions.Remove(perm);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ─── Groups ────────────────────────────────────────────

    [HttpGet("groups")]
    public async Task<IActionResult> GetAllGroups()
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        return Ok(await _db.PermissionGroups.Where(g => g.OrganizationId == oid)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                permissionCount = _db.GroupPermissions.Count(gp => gp.GroupId == g.Id)
            })
            .OrderBy(g => g.Name).ToListAsync());
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var group = new PermissionGroup
        {
            Name = req.Name,
            Description = req.Description,
            OrganizationId = oid.Value
        };

        _db.PermissionGroups.Add(group);
        await _db.SaveChangesAsync();

        // Assign permissions if provided
        if (req.PermissionIds != null && req.PermissionIds.Count > 0)
        {
            foreach (var permId in req.PermissionIds)
            {
                _db.GroupPermissions.Add(new GroupPermission { GroupId = group.Id, PermissionId = permId });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { group.Id, group.Name });
    }

    [HttpPut("groups/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupReq req)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var group = await _db.PermissionGroups.FirstOrDefaultAsync(g => g.Id == id && g.OrganizationId == oid);
        if (group == null) return NotFound(new { message = "Grupo não encontrado" });

        if (req.Name != null) group.Name = req.Name;
        if (req.Description != null) group.Description = req.Description;

        // Update permissions if provided
        if (req.PermissionIds != null)
        {
            var existingGps = await _db.GroupPermissions.Where(gp => gp.GroupId == id).ToListAsync();
            _db.GroupPermissions.RemoveRange(existingGps);

            foreach (var permId in req.PermissionIds)
            {
                _db.GroupPermissions.Add(new GroupPermission { GroupId = id, PermissionId = permId });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { group.Id, group.Name });
    }

    [HttpDelete("groups/{id:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var oid = OrgId;
        if (oid == null) return BadRequest(new { message = "Sem organização" });

        var group = await _db.PermissionGroups.FirstOrDefaultAsync(g => g.Id == id && g.OrganizationId == oid);
        if (group == null) return NotFound(new { message = "Grupo não encontrado" });

        // Remove all group-permission associations
        var gps = await _db.GroupPermissions.Where(gp => gp.GroupId == id).ToListAsync();
        _db.GroupPermissions.RemoveRange(gps);

        // Remove all user-group associations
        var ugs = await _db.UserGroups.Where(ug => ug.GroupId == id).ToListAsync();
        _db.UserGroups.RemoveRange(ugs);

        _db.PermissionGroups.Remove(group);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ─── User-Group assignments ────────────────────────────

    [HttpPost("groups/{groupId:guid}/users")]
    public async Task<IActionResult> AssignUserToGroup(Guid groupId, [FromBody] AssignUserReq req)
    {
        if (await _db.UserGroups.AnyAsync(ug => ug.UserId == req.UserId && ug.GroupId == groupId))
            return BadRequest(new { message = "Usuário já pertence a este grupo" });

        _db.UserGroups.Add(new UserGroup { UserId = req.UserId, GroupId = groupId });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Usuário adicionado ao grupo" });
    }

    [HttpDelete("groups/{groupId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUserFromGroup(Guid groupId, Guid userId)
    {
        var ug = await _db.UserGroups.FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == groupId);
        if (ug == null) return NotFound(new { message = "Associação não encontrada" });

        _db.UserGroups.Remove(ug);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("groups/{groupId:guid}/users")]
    public async Task<IActionResult> GetGroupUsers(Guid groupId)
    {
        var userIds = await _db.UserGroups.Where(ug => ug.GroupId == groupId).Select(ug => ug.UserId).ToListAsync();
        var users = await _db.Users.Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Email, u.Role }).ToListAsync();
        return Ok(users);
    }
}

public record CreatePermissionReq(string Resource, string Action, string Name);
public record CreateGroupReq(string Name, string? Description, List<Guid>? PermissionIds);
public record UpdateGroupReq(string? Name, string? Description, List<Guid>? PermissionIds);
public record AssignUserReq(Guid UserId);
