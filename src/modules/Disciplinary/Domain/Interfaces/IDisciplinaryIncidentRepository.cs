using Giglio.EduCore.Disciplinary.Domain.Entities;

namespace Giglio.EduCore.Disciplinary.Domain.Interfaces;

public record DisciplinaryFilter(
    Guid? ClassId = null,
    string? Type = null,
    string? Status = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null);

public interface IDisciplinaryIncidentRepository
{
    Task<DisciplinaryIncident?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DisciplinaryIncident>> GetAllAsync(DisciplinaryFilter? filter = null, CancellationToken ct = default);
    Task<IReadOnlyList<DisciplinaryIncident>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<DisciplinaryIncident>> GetByClassAsync(Guid classId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DisciplinaryIncident incident, CancellationToken ct = default);
    Task UpdateAsync(DisciplinaryIncident incident, CancellationToken ct = default);
    Task DeleteAsync(DisciplinaryIncident incident, CancellationToken ct = default);
}
