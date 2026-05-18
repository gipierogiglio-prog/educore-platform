using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Domain.Entities;

public class FinancialPlan
{
    public Guid Id { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public decimal BaseValue { get; private set; }
    public int DueDay { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public DiscountType? DiscountType { get; private set; }
    public int StartMonth { get; private set; }
    public int StartYear { get; private set; }
    public int? EndMonth { get; private set; }
    public int? EndYear { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private FinancialPlan() { }

    public FinancialPlan(
        Guid enrollmentId,
        decimal baseValue,
        int dueDay,
        int startMonth,
        int startYear,
        decimal? discountPercent = null,
        DiscountType? discountType = null,
        int? endMonth = null,
        int? endYear = null)
    {
        Id = Guid.NewGuid();
        EnrollmentId = enrollmentId;
        SetBaseValue(baseValue);
        SetDueDay(dueDay);
        SetPeriod(startMonth, startYear, endMonth, endYear);
        SetDiscount(discountPercent, discountType);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetBaseValue(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Base value must be greater than zero", nameof(value));
        BaseValue = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDueDay(int day)
    {
        if (day < 1 || day > 31)
            throw new ArgumentException("Due day must be between 1 and 31", nameof(day));
        DueDay = day;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDiscount(decimal? percent, DiscountType? type)
    {
        if (percent.HasValue != type.HasValue)
            throw new ArgumentException("Discount percent and type must be provided together");

        if (percent.HasValue && percent.Value < 0)
            throw new ArgumentException("Discount percent cannot be negative", nameof(percent));

        if (type == Enums.DiscountType.Fixed && percent.HasValue && percent.Value > BaseValue)
            throw new ArgumentException("Fixed discount cannot be greater than base value");

        if (type == Enums.DiscountType.Percentage && percent.HasValue && percent.Value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%");

        DiscountPercent = percent;
        DiscountType = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPeriod(int startMonth, int startYear, int? endMonth, int? endYear)
    {
        if (startMonth < 1 || startMonth > 12)
            throw new ArgumentException("Start month must be between 1 and 12", nameof(startMonth));
        if (endMonth.HasValue && (endMonth.Value < 1 || endMonth.Value > 12))
            throw new ArgumentException("End month must be between 1 and 12", nameof(endMonth));

        if (endYear.HasValue && endMonth.HasValue)
        {
            var start = new DateTime(startYear, startMonth, 1);
            var end = new DateTime(endYear.Value, endMonth.Value, 1);
            if (end <= start)
                throw new ArgumentException("End period must be after start period");
        }

        StartMonth = startMonth;
        StartYear = startYear;
        EndMonth = endMonth;
        EndYear = endYear;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateMonthlyValue()
    {
        if (!DiscountPercent.HasValue || !DiscountType.HasValue)
            return BaseValue;

        return DiscountType switch
        {
            Enums.DiscountType.Percentage => BaseValue * (1 - DiscountPercent.Value / 100m),
            Enums.DiscountType.Fixed => Math.Max(0, BaseValue - DiscountPercent.Value),
            _ => BaseValue
        };
    }

    public DateTime GetDueDateForMonth(int month, int year)
    {
        int lastDay = DateTime.DaysInMonth(year, month);
        int day = Math.Min(DueDay, lastDay);
        return new DateTime(year, month, day);
    }
}
