using Giglio.EduCore.Academic.Domain.Entities;

namespace Giglio.EduCore.Academic.Domain.Interfaces;

public interface ICurriculumMatrixRepository
{
    Task<CurriculumMatrix?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CurriculumMatrix>> GetBySeriesAsync(Guid seriesId, CancellationToken ct = default);
    Task<bool> SubjectExistsInSeriesAsync(Guid seriesId, Guid subjectId, CancellationToken ct = default);
    Task AddAsync(CurriculumMatrix entry, CancellationToken ct = default);
    Task DeleteAsync(CurriculumMatrix entry, CancellationToken ct = default);
    Task<int> GetSubjectCountAsync(Guid seriesId, CancellationToken ct = default);
}