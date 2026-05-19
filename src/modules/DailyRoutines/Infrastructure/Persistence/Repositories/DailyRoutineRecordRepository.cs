using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Repositories;

public class DailyRoutineRecordRepository : IDailyRoutineRecordRepository
{
    private readonly DailyRoutinesDbContext _context;

    public DailyRoutineRecordRepository(DailyRoutinesDbContext context)
    {
        _context = context;
    }

    public async Task<DailyRoutineRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DailyRoutineRecords
            .Include(x => x.ClassRoutine)
                .ThenInclude(cr => cr.Routine)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<DailyRoutineRecord>> GetByClassRoutineAsync(
        Guid classRoutineId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = _context.DailyRoutineRecords
            .Include(x => x.ClassRoutine)
                .ThenInclude(cr => cr.Routine)
            .Where(x => x.ClassRoutineId == classRoutineId)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(x => x.RecordDate >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(x => x.RecordDate <= endDate.Value.Date);

        return await query
            .OrderByDescending(x => x.RecordDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DailyRoutineRecord>> GetByClassAndDateAsync(
        Guid classId,
        DateTime date,
        CancellationToken ct = default)
        => await _context.DailyRoutineRecords
            .Include(x => x.ClassRoutine)
                .ThenInclude(cr => cr.Routine)
            .Where(x => x.ClassRoutine!.ClassId == classId
                     && x.RecordDate == date.Date)
            .OrderBy(x => x.ClassRoutine!.StartTime)
            .ToListAsync(ct);

    public async Task<DailyRoutineRecord?> GetByClassRoutineAndDateAsync(
        Guid classRoutineId,
        DateTime date,
        CancellationToken ct = default)
        => await _context.DailyRoutineRecords
            .Include(x => x.ClassRoutine)
                .ThenInclude(cr => cr.Routine)
            .FirstOrDefaultAsync(
                x => x.ClassRoutineId == classRoutineId
                  && x.RecordDate == date.Date, ct);

    public async Task<IReadOnlyList<DailyRoutineRecord>> GetByDateRangeAsync(
        Guid classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
        => await _context.DailyRoutineRecords
            .Include(x => x.ClassRoutine)
                .ThenInclude(cr => cr.Routine)
            .Where(x => x.ClassRoutine!.ClassId == classId
                     && x.RecordDate >= startDate.Date
                     && x.RecordDate <= endDate.Date)
            .OrderBy(x => x.RecordDate)
            .ThenBy(x => x.ClassRoutine!.StartTime)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.DailyRoutineRecords.AnyAsync(x => x.Id == id, ct);

    public async Task AddAsync(DailyRoutineRecord record, CancellationToken ct = default)
    {
        await _context.DailyRoutineRecords.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DailyRoutineRecord record, CancellationToken ct = default)
    {
        _context.DailyRoutineRecords.Update(record);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DailyRoutineRecord record, CancellationToken ct = default)
    {
        _context.DailyRoutineRecords.Remove(record);
        await _context.SaveChangesAsync(ct);
    }
}
