using Giglio.EduCore.Financial.Domain.Entities;

namespace Giglio.EduCore.Financial.Domain.Interfaces;

public interface IMonthlyChargeRepository
{
    Task<MonthlyCharge?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid financialPlanId, int month, int year, CancellationToken ct = default);
    Task<List<MonthlyCharge>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<List<MonthlyCharge>> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken ct = default);
    Task<List<MonthlyCharge>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<List<MonthlyCharge>> GetOverdueCandidatesAsync(CancellationToken ct = default);
    Task<List<MonthlyCharge>> GetFilteredAsync(
        Guid? studentId = null,
        Guid? planId = null,
        int? startMonth = null,
        int? startYear = null,
        int? endMonth = null,
        int? endYear = null,
        List<string>? statusFilter = null,
        int page = 1,
        int pageSize = 12,
        CancellationToken ct = default);
    Task<int> GetFilteredCountAsync(
        Guid? studentId = null,
        Guid? planId = null,
        int? startMonth = null,
        int? startYear = null,
        int? endMonth = null,
        int? endYear = null,
        List<string>? statusFilter = null,
        CancellationToken ct = default);
    void Add(MonthlyCharge charge);
    void AddRange(IEnumerable<MonthlyCharge> charges);
    void Update(MonthlyCharge charge);
    Task SaveChangesAsync(CancellationToken ct = default);
}
