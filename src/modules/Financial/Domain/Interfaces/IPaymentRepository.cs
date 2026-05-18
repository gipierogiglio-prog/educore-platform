using Giglio.EduCore.Financial.Domain.Entities;

namespace Giglio.EduCore.Financial.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Payment>> GetByChargeIdAsync(Guid monthlyChargeId, bool includeCancelled = false,
        int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<int> GetCountByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default);
    Task<decimal> GetActiveTotalByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default);
    Task<bool> HasActivePaymentsByChargeIdAsync(Guid monthlyChargeId, CancellationToken ct = default);
    void Add(Payment payment);
}
