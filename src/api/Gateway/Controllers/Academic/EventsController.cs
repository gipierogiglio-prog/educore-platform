using Educore.Database;
using Educore.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Educore.Api.Controllers.Academic;

[ApiController]
[Route("api/academic/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) => _db = db;
    private Guid? OrgId => Guid.TryParse(User.FindFirstValue("organizationId"), out var id) ? id : null;

    // === LISTAR eventos (com filtros opcionais) ===
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? classId)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });

        var query = _db.AcademicEvents
            .Where(e => e.OrganizationId == oid)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(e => e.EventType == type);

        if (from.HasValue)
            query = query.Where(e => e.StartDate >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.StartDate <= to.Value);

        if (classId.HasValue)
            query = query.Where(e => e.ClassId == classId.Value || e.ClassId == null);

        var events = await query
            .OrderBy(e => e.StartDate)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Description,
                e.EventType,
                e.StartDate,
                e.EndDate,
                e.AllDay,
                e.Color,
                e.ClassId,
                e.SubjectId,
                e.CreatedAt,
                e.UpdatedAt
            })
            .ToListAsync();

        return Ok(events);
    }

    // === OBTER evento por ID ===
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });

        var evt = await _db.AcademicEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == oid);

        if (evt == null)
            return NotFound(new { message = "Evento não encontrado" });

        return Ok(evt);
    }

    // === CRIAR evento ===
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { message = "Título é obrigatório" });

        if (req.StartDate == default)
            return BadRequest(new { message = "Data de início é obrigatória" });

        if (req.EndDate.HasValue && req.EndDate.Value < req.StartDate)
            return BadRequest(new { message = "Data final não pode ser anterior à data inicial" });

        var validTypes = new[] { "holiday", "exam", "activity", "event", "vacation" };
        var eventType = !string.IsNullOrEmpty(req.EventType) && validTypes.Contains(req.EventType)
            ? req.EventType : "event";

        var evt = new AcademicEvent
        {
            Title = req.Title.Trim(),
            Description = req.Description?.Trim(),
            EventType = eventType,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            AllDay = req.AllDay,
            Color = req.Color,
            OrganizationId = oid.Value,
            ClassId = req.ClassId,
            SubjectId = req.SubjectId
        };

        _db.AcademicEvents.Add(evt);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = evt.Id }, evt);
    }

    // === ATUALIZAR evento ===
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventReq req)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });

        var evt = await _db.AcademicEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == oid);

        if (evt == null)
            return NotFound(new { message = "Evento não encontrado" });

        if (!string.IsNullOrWhiteSpace(req.Title))
            evt.Title = req.Title.Trim();

        if (req.Description != null)
            evt.Description = req.Description?.Trim();

        if (!string.IsNullOrEmpty(req.EventType))
        {
            var validTypes = new[] { "holiday", "exam", "activity", "event", "vacation" };
            if (validTypes.Contains(req.EventType))
                evt.EventType = req.EventType;
        }

        if (req.StartDate.HasValue && req.StartDate.Value != default)
            evt.StartDate = req.StartDate.Value;

        if (req.EndDate.HasValue)
        {
            if (req.EndDate.Value < evt.StartDate)
                return BadRequest(new { message = "Data final não pode ser anterior à data inicial" });
            evt.EndDate = req.EndDate;
        }

        if (req.AllDay.HasValue)
            evt.AllDay = req.AllDay.Value;

        if (req.Color != null)
            evt.Color = req.Color;

        if (req.ClassId.HasValue)
            evt.ClassId = req.ClassId;

        if (req.SubjectId.HasValue)
            evt.SubjectId = req.SubjectId;

        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(evt);
    }

    // === REMOVER evento ===
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        var oid = OrgId; if (oid == null) return BadRequest(new { message = "Sem organização" });

        var evt = await _db.AcademicEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == oid);

        if (evt == null)
            return NotFound(new { message = "Evento não encontrado" });

        _db.AcademicEvents.Remove(evt);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Evento removido" });
    }
}

// === Request DTOs ===
public record CreateEventReq(
    string Title,
    string? Description,
    string? EventType,
    DateTime StartDate,
    DateTime? EndDate,
    bool AllDay = false,
    string? Color = null,
    Guid? ClassId = null,
    Guid? SubjectId = null);

public record UpdateEventReq(
    string? Title = null,
    string? Description = null,
    string? EventType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool? AllDay = null,
    string? Color = null,
    Guid? ClassId = null,
    Guid? SubjectId = null);
