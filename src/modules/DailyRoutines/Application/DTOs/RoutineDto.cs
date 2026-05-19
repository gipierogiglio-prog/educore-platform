using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Application.DTOs;

public record RoutineDto(
    Guid Id,
    string Name,
    string? Description,
    RoutineCategory Category,
    int ExpectedDurationMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
