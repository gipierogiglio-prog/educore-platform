using Giglio.EduCore.Organization.Domain.Entities;

namespace Giglio.EduCore.Organization.Domain.Interfaces;

public interface ISchoolUnitRepository
{
    Task<SchoolUnit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolUnit>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task AddAsync(SchoolUnit unit, CancellationToken ct = default);
    Task UpdateAsync(SchoolUnit unit, CancellationToken ct = default);
    Task DeleteAsync(SchoolUnit unit, CancellationToken ct = default);
}