using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.DTOs;

public record DailyRoutineRecordDto(
    Guid Id,
    Guid ClassRoutineId,
    string? RoutineName,
    string? RoutineCategory,
    DateTime RecordDate,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    RoutineRecordStatus Status,
    string? Notes,
    Guid? TeacherId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
