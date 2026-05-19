using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Repositories;

public class RoutineRepository : IRoutineRepository
{
    private readonly DailyRoutinesDbContext _context;

    public RoutineRepository(DailyRoutinesDbContext context)
    {
        _context = context;
    }

    public async Task<Routine?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Routines
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Routine>> GetAllAsync(
        bool? activeOnly = null,
        RoutineCategory? category = null,
        CancellationToken ct = default)
    {
        var query = _context.Routines.AsQueryable();

        if (activeOnly.HasValue)
            query = query.Where(x => x.IsActive == activeOnly.Value);

        if (category.HasValue)
            query = query.Where(x => x.Category == category.Value);

        return await query
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Routines.AnyAsync(x => x.Id == id, ct);

    public async Task AddAsync(Routine routine, CancellationToken ct = default)
    {
        await _context.Routines.AddAsync(routine, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Routine routine, CancellationToken ct = default)
    {
        _context.Routines.Update(routine);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Routine routine, CancellationToken ct = default)
    {
        _context.Routines.Remove(routine);
        await _context.SaveChangesAsync(ct);
    }
}
