using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Repositories;

public class ClassRoutineRepository : IClassRoutineRepository
{
    private readonly DailyRoutinesDbContext _context;

    public ClassRoutineRepository(DailyRoutinesDbContext context)
    {
        _context = context;
    }

    public async Task<ClassRoutine?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ClassRoutines
            .Include(x => x.Routine)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ClassRoutine>> GetByClassAsync(
        Guid classId,
        WeekDay? weekDay = null,
        CancellationToken ct = default)
    {
        var query = _context.ClassRoutines
            .Include(x => x.Routine)
            .Where(x => x.ClassId == classId)
            .AsQueryable();

        if (weekDay.HasValue)
            query = query.Where(x => x.WeekDay == weekDay.Value);

        return await query
            .OrderBy(x => x.WeekDay)
            .ThenBy(x => x.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassRoutine>> GetByRoutineAsync(Guid routineId, CancellationToken ct = default)
        => await _context.ClassRoutines
            .Include(x => x.Routine)
            .Where(x => x.RoutineId == routineId)
            .OrderBy(x => x.ClassId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ClassRoutine>> GetByClassAndDayAsync(
        Guid classId,
        WeekDay weekDay,
        CancellationToken ct = default)
        => await _context.ClassRoutines
            .Include(x => x.Routine)
            .Where(x => x.ClassId == classId && x.WeekDay == weekDay && x.IsActive)
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.ClassRoutines.AnyAsync(x => x.Id == id, ct);

    public async Task AddAsync(ClassRoutine classRoutine, CancellationToken ct = default)
    {
        await _context.ClassRoutines.AddAsync(classRoutine, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ClassRoutine classRoutine, CancellationToken ct = default)
    {
        _context.ClassRoutines.Update(classRoutine);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ClassRoutine classRoutine, CancellationToken ct = default)
    {
        _context.ClassRoutines.Remove(classRoutine);
        await _context.SaveChangesAsync(ct);
    }
}
