using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Interfaces;

public interface IRoutineRepository
{
    Task<Routine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Routine>> GetAllAsync(bool? activeOnly = null, RoutineCategory? category = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Routine routine, CancellationToken ct = default);
    Task UpdateAsync(Routine routine, CancellationToken ct = default);
    Task DeleteAsync(Routine routine, CancellationToken ct = default);
}
