using Educore.Core.Entities;
using Educore.Database;
using Giglio.EduCore.DailyRoutines.Application.DTOs;
using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;
using Giglio.EduCore.DailyRoutines.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Application.Queries;

/// <summary>
/// Query para obter o status das rotinas de uma turma em uma data específica.
/// Task #202: Backend - Query de status de rotinas.
/// </summary>
public class GetRoutineStatusQuery
{
    private readonly DailyRoutinesDbContext _db;
    private readonly AppDbContext _appDb;

    public GetRoutineStatusQuery(DailyRoutinesDbContext db, AppDbContext appDb)
    {
        _db = db;
        _appDb = appDb;
    }

    public async Task<RoutineStatusDto?> ExecuteAsync(
        Guid classId,
        DateTime date,
        CancellationToken ct = default)
    {
        var classEntity = await _appDb.Classes
            .FirstOrDefaultAsync(c => c.Id == classId, ct);

        if (classEntity is null) return null;

        var weekDay = (WeekDay)((int)date.DayOfWeek);

        var classRoutines = await _db.ClassRoutines
            .Include(cr => cr.Routine)
            .Where(cr => cr.ClassId == classId
                      && cr.WeekDay == weekDay
                      && cr.IsActive)
            .OrderBy(cr => cr.StartTime)
            .ToListAsync(ct);

        var classRoutineIds = classRoutines.Select(cr => cr.Id).ToList();

        var records = await _db.DailyRoutineRecords
            .Where(r => classRoutineIds.Contains(r.ClassRoutineId)
                     && r.RecordDate == date.Date)
            .ToListAsync(ct);

        var recordsByClassRoutine = records
            .GroupBy(r => r.ClassRoutineId)
            .ToDictionary(g => g.Key, g => g.First());

        int total = classRoutines.Count;
        int completed = records.Count(r => r.Status == RoutineRecordStatus.Completed);
        int inProgress = records.Count(r => r.Status == RoutineRecordStatus.InProgress);
        int cancelled = records.Count(r => r.Status == RoutineRecordStatus.Cancelled);
        int pending = total - completed - inProgress - cancelled;

        double completionPct = total > 0
            ? Math.Round((double)completed / total * 100, 1)
            : 0;

        var items = classRoutines.Select(cr =>
        {
            var record = recordsByClassRoutine.GetValueOrDefault(cr.Id);
            return new RoutineStatusItemDto(
                cr.Id,
                cr.RoutineId,
                cr.Routine?.Name ?? "N/A",
                cr.Routine?.Category.ToString() ?? "N/A",
                cr.StartTime,
                cr.DurationMinutes,
                record?.Status.ToString() ?? "Pending",
                record?.StartTime,
                record?.EndTime,
                record?.Id
            );
        }).ToList();

        return new RoutineStatusDto(
            classId,
            classEntity.Name,
            date.Date,
            total,
            completed,
            inProgress,
            pending,
            cancelled,
            completionPct,
            items);
    }
}
