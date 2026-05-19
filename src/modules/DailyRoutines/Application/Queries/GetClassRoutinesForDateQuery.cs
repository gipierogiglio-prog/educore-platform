using Giglio.EduCore.DailyRoutines.Application.DTOs;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Application.Queries;

/// <summary>
/// Query para listar as rotinas programadas de uma turma em uma data,
/// incluindo os registros existentes para lançamento.
/// </summary>
public class GetClassRoutinesForDateQuery
{
    private readonly DailyRoutinesDbContext _db;

    public GetClassRoutinesForDateQuery(DailyRoutinesDbContext db)
    {
        _db = db;
    }

    public async Task<List<DailyRoutineRecordDto>> ExecuteAsync(
        Guid classId,
        DateTime date,
        CancellationToken ct = default)
    {
        var weekDay = (WeekDay)((int)date.DayOfWeek);

        var classRoutines = await _db.ClassRoutines
            .Include(cr => cr.Routine)
            .Where(cr => cr.ClassId == classId
                      && cr.WeekDay == weekDay
                      && cr.IsActive)
            .ToListAsync(ct);

        var classRoutineIds = classRoutines.Select(cr => cr.Id).ToList();

        var existingRecords = await _db.DailyRoutineRecords
            .Where(r => classRoutineIds.Contains(r.ClassRoutineId)
                     && r.RecordDate == date.Date)
            .ToListAsync(ct);

        var recordsByCr = existingRecords
            .GroupBy(r => r.ClassRoutineId)
            .ToDictionary(g => g.Key, g => g.First());

        return classRoutines.Select(cr =>
        {
            var record = recordsByCr.GetValueOrDefault(cr.Id);
            var routine = cr.Routine;
            return new DailyRoutineRecordDto(
                record?.Id ?? Guid.Empty,
                cr.Id,
                routine?.Name ?? "N/A",
                routine?.Category.ToString() ?? "N/A",
                date.Date,
                record?.StartTime,
                record?.EndTime,
                record?.Status ?? RoutineRecordStatus.Pending,
                record?.Notes,
                record?.TeacherId,
                record?.CreatedAt ?? DateTime.UtcNow,
                record?.UpdatedAt
            );
        }).ToList();
    }
}
