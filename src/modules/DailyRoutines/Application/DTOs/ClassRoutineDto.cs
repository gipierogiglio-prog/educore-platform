using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.DTOs;

public record ClassRoutineDto(
    Guid Id,
    Guid ClassId,
    Guid RoutineId,
    string? RoutineName,
    string? RoutineCategory,
    WeekDay WeekDay,
    TimeSpan StartTime,
    int DurationMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
