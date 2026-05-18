using Giglio.EduCore.Academic.Domain.Entities;
using Giglio.EduCore.Academic.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Academic.Infrastructure.Persistence.Repositories;

public class CurriculumMatrixRepository : ICurriculumMatrixRepository
{
    private readonly AcademicDbContext _context;

    public CurriculumMatrixRepository(AcademicDbContext context)
    {
        _context = context;
    }

    public async Task<CurriculumMatrix?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.CurriculumMatrices
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CurriculumMatrix>> GetBySeriesAsync(Guid seriesId, CancellationToken ct = default)
        => await _context.CurriculumMatrices
            .Where(x => x.SeriesId == seriesId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<bool> SubjectExistsInSeriesAsync(Guid seriesId, Guid subjectId, CancellationToken ct = default)
        => await _context.CurriculumMatrices
            .AnyAsync(x => x.SeriesId == seriesId && x.SubjectId == subjectId, ct);

    public async Task AddAsync(CurriculumMatrix entry, CancellationToken ct = default)
    {
        await _context.CurriculumMatrices.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(CurriculumMatrix entry, CancellationToken ct = default)
    {
        _context.CurriculumMatrices.Remove(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetSubjectCountAsync(Guid seriesId, CancellationToken ct = default)
        => await _context.CurriculumMatrices
            .CountAsync(x => x.SeriesId == seriesId, ct);
}