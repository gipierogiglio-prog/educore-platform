using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Interfaces;

public interface IClassRoutineRepository
{
    Task<ClassRoutine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRoutine>> GetByClassAsync(Guid classId, WeekDay? weekDay = null, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRoutine>> GetByRoutineAsync(Guid routineId, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRoutine>> GetByClassAndDayAsync(Guid classId, WeekDay weekDay, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ClassRoutine classRoutine, CancellationToken ct = default);
    Task UpdateAsync(ClassRoutine classRoutine, CancellationToken ct = default);
    Task DeleteAsync(ClassRoutine classRoutine, CancellationToken ct = default);
}
