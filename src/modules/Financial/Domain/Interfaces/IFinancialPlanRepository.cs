using Giglio.EduCore.Financial.Domain.Entities;

namespace Giglio.EduCore.Financial.Domain.Interfaces;

public interface IFinancialPlanRepository
{
    Task<FinancialPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialPlan>> GetByEnrollmentAsync(Guid enrollmentId, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialPlan>> GetAllAsync(bool? activeOnly = null, CancellationToken ct = default);
    Task AddAsync(FinancialPlan plan, CancellationToken ct = default);
    Task UpdateAsync(FinancialPlan plan, CancellationToken ct = default);
    Task<bool> HasPaidChargesAsync(Guid planId, CancellationToken ct = default);
}
