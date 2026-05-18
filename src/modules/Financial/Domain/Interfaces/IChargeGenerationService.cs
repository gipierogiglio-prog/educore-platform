using Giglio.EduCore.Financial.Domain.Entities;

namespace Giglio.EduCore.Financial.Domain.Interfaces;

public interface IChargeGenerationService
{
    Task<List<MonthlyCharge>> GenerateChargesAsync(
        FinancialPlan plan,
        CancellationToken ct = default);
}
