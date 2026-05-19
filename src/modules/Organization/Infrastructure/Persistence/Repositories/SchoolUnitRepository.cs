using Giglio.EduCore.Organization.Domain.Entities;
using Giglio.EduCore.Organization.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Organization.Infrastructure.Persistence.Repositories;

public class SchoolUnitRepository : ISchoolUnitRepository
{
    private readonly OrganizationDbContext _context;

    public SchoolUnitRepository(OrganizationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolUnit?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.SchoolUnits.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<SchoolUnit>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.SchoolUnits.AsQueryable();

        if (activeOnly.HasValue)
            query = query.Where(x => x.IsActive == activeOnly.Value);

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SchoolUnit unit, CancellationToken ct = default)
    {
        await _context.SchoolUnits.AddAsync(unit, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SchoolUnit unit, CancellationToken ct = default)
    {
        _context.SchoolUnits.Update(unit);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(SchoolUnit unit, CancellationToken ct = default)
    {
        _context.SchoolUnits.Remove(unit);
        await _context.SaveChangesAsync(ct);
    }
}