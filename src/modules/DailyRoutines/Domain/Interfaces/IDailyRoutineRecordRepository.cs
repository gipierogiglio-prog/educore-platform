using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Interfaces;

public interface IDailyRoutineRecordRepository
{
    Task<DailyRoutineRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DailyRoutineRecord>> GetByClassRoutineAsync(Guid classRoutineId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailyRoutineRecord>> GetByClassAndDateAsync(Guid classId, DateTime date, CancellationToken ct = default);
    Task<DailyRoutineRecord?> GetByClassRoutineAndDateAsync(Guid classRoutineId, DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<DailyRoutineRecord>> GetByDateRangeAsync(Guid classId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DailyRoutineRecord record, CancellationToken ct = default);
    Task UpdateAsync(DailyRoutineRecord record, CancellationToken ct = default);
    Task DeleteAsync(DailyRoutineRecord record, CancellationToken ct = default);
}
