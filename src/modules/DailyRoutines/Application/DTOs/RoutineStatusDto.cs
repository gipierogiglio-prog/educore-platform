namespace Giglio.EduCore.DailyRoutines.Application.DTOs;

public record RoutineStatusDto(
    Guid ClassId,
    string ClassName,
    DateTime Date,
    int TotalRoutines,
    int CompletedRoutines,
    int InProgressRoutines,
    int PendingRoutines,
    int CancelledRoutines,
    double CompletionPercentage,
    List<RoutineStatusItemDto> Items);

public record RoutineStatusItemDto(
    Guid ClassRoutineId,
    Guid RoutineId,
    string RoutineName,
    string RoutineCategory,
    TimeSpan ScheduledTime,
    int DurationMinutes,
    string Status,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    Guid? RecordId);
