using Giglio.EduCore.Academic.Domain.Entities;

namespace Giglio.EduCore.Academic.Domain.Interfaces;

public interface ISeriesRepository
{
    Task<Series?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Series>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task<IReadOnlyList<Series>> GetByCourseAsync(Guid courseId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Series series, CancellationToken ct = default);
    Task UpdateAsync(Series series, CancellationToken ct = default);
    Task DeleteAsync(Series series, CancellationToken ct = default);
}