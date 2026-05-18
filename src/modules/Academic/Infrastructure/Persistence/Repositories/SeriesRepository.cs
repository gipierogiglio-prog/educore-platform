using Giglio.EduCore.Academic.Domain.Entities;
using Giglio.EduCore.Academic.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Academic.Infrastructure.Persistence.Repositories;

public class SeriesRepository : ISeriesRepository
{
    private readonly AcademicDbContext _context;

    public SeriesRepository(AcademicDbContext context)
    {
        _context = context;
    }

    public async Task<Series?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Series
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Series>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.Series
            .Include(x => x.Course)
            .AsQueryable();

        if (activeOnly.HasValue)
            query = query.Where(x => x.IsActive == activeOnly.Value);

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Series>> GetByCourseAsync(Guid courseId, CancellationToken ct = default)
        => await _context.Series
            .Include(x => x.Course)
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Series.AnyAsync(x => x.Id == id, ct);

    public async Task AddAsync(Series series, CancellationToken ct = default)
    {
        await _context.Series.AddAsync(series, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Series series, CancellationToken ct = default)
    {
        _context.Series.Update(series);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Series series, CancellationToken ct = default)
    {
        _context.Series.Remove(series);
        await _context.SaveChangesAsync(ct);
    }
}