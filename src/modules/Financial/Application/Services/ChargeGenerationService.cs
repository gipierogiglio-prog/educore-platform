using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Domain.Interfaces;

namespace Giglio.EduCore.Financial.Application.Services;

public class ChargeGenerationService : IChargeGenerationService
{
    private readonly IMonthlyChargeRepository _chargeRepo;

    public ChargeGenerationService(IMonthlyChargeRepository chargeRepo)
    {
        _chargeRepo = chargeRepo;
    }

    public async Task<List<MonthlyCharge>> GenerateChargesAsync(
        FinancialPlan plan,
        CancellationToken ct = default)
    {
        var charges = new List<MonthlyCharge>();
        var today = DateTime.UtcNow;
        int currentMonth = today.Month;
        int currentYear = today.Year;

        // Limite: mês atual + 3 meses projetados
        int limitMonth = currentMonth + 3;
        int limitYear = currentYear;
        if (limitMonth > 12)
        {
            limitMonth -= 12;
            limitYear += 1;
        }

        int startMonth = plan.StartMonth;
        int startYear = plan.StartYear;

        int endMonth = limitMonth;
        int endYear = limitYear;

        // Se o plano tem fim, respeitar o menor
        if (plan.EndYear.HasValue && plan.EndMonth.HasValue)
        {
            if (plan.EndYear.Value < endYear ||
                (plan.EndYear.Value == endYear && plan.EndMonth.Value < endMonth))
            {
                endMonth = plan.EndMonth.Value;
                endYear = plan.EndYear.Value;
            }
        }

        int m = startMonth;
        int y = startYear;
        while (y < endYear || (y == endYear && m <= endMonth))
        {
            // Pular meses passados (não gerar retroativo)
            if (y < currentYear || (y == currentYear && m < currentMonth))
            {
                m++;
                if (m > 12) { m = 1; y++; }
                continue;
            }

            // Verificar duplicata
            bool exists = await _chargeRepo.ExistsAsync(plan.Id, m, y, ct);
            if (!exists)
            {
                var value = CalculateChargeValue(plan.BaseValue, plan.DiscountPercent, plan.DiscountType);
                var dueDate = ResolveDueDate(y, m, plan.DueDay);
                charges.Add(new MonthlyCharge(plan.Id, m, y, value, dueDate));
            }

            m++;
            if (m > 12) { m = 1; y++; }
        }

        return charges;
    }

    private static decimal CalculateChargeValue(
        decimal baseValue,
        decimal? discountPercent,
        DiscountType? discountType)
    {
        if (!discountPercent.HasValue || !discountType.HasValue)
            return baseValue;

        return discountType switch
        {
            DiscountType.Percentage =>
                Math.Round(baseValue - (baseValue * discountPercent.Value / 100m), 2),
            DiscountType.Fixed =>
                Math.Max(0, baseValue - discountPercent.Value),
            _ => baseValue
        };
    }

    private static DateTime ResolveDueDate(int year, int month, int dueDay)
    {
        int lastDay = DateTime.DaysInMonth(year, month);
        int effectiveDay = Math.Min(dueDay, lastDay);
        return new DateTime(year, month, effectiveDay, 0, 0, 0, DateTimeKind.Utc);
    }
}
