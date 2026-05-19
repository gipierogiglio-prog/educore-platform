using Giglio.EduCore.Disciplinary.Domain.Entities;
using Giglio.EduCore.Disciplinary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Disciplinary.Infrastructure.Persistence.Repositories;

public class DisciplinaryIncidentRepository : IDisciplinaryIncidentRepository
{
    private readonly DisciplinaryDbContext _context;

    public DisciplinaryIncidentRepository(DisciplinaryDbContext context)
    {
        _context = context;
    }

    public async Task<DisciplinaryIncident?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DisciplinaryIncidents
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<DisciplinaryIncident>> GetAllAsync(DisciplinaryFilter? filter = null, CancellationToken ct = default)
    {
        var query = _context.DisciplinaryIncidents.AsQueryable();

        if (filter is not null)
        {
            if (filter.ClassId.HasValue)
                query = query.Where(x => x.ClassId == filter.ClassId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Type))
                query = query.Where(x => x.Type == filter.Type.Trim().ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(x => x.Status == filter.Status.Trim().ToLowerInvariant());

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.OccurredAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.OccurredAt <= filter.DateTo.Value);
        }

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DisciplinaryIncident>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => await _context.DisciplinaryIncidents
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DisciplinaryIncident>> GetByClassAsync(Guid classId, CancellationToken ct = default)
        => await _context.DisciplinaryIncidents
            .Where(x => x.ClassId == classId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.DisciplinaryIncidents.AnyAsync(x => x.Id == id, ct);

    public async Task AddAsync(DisciplinaryIncident incident, CancellationToken ct = default)
    {
        await _context.DisciplinaryIncidents.AddAsync(incident, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DisciplinaryIncident incident, CancellationToken ct = default)
    {
        _context.DisciplinaryIncidents.Update(incident);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DisciplinaryIncident incident, CancellationToken ct = default)
    {
        _context.DisciplinaryIncidents.Remove(incident);
        await _context.SaveChangesAsync(ct);
    }
}
