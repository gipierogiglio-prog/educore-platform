using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Domain.Entities;

public class MonthlyCharge
{
    public Guid Id { get; private set; }
    public Guid FinancialPlanId { get; private set; }
    public int ReferenceMonth { get; private set; }
    public int ReferenceYear { get; private set; }
    public decimal Value { get; private set; }
    public DateTime DueDate { get; private set; }
    public ChargeStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // RowVersion para concorrência otimista
    public byte[] RowVersion { get; private set; } = [];

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private MonthlyCharge() { }

    public MonthlyCharge(
        Guid financialPlanId,
        int referenceMonth,
        int referenceYear,
        decimal value,
        DateTime dueDate)
    {
        Id = Guid.NewGuid();
        FinancialPlanId = financialPlanId;
        SetReference(referenceMonth, referenceYear);
        SetValue(value);
        DueDate = dueDate;
        Status = ChargeStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Payments = new List<Payment>();
    }

    public void SetReference(int month, int year)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("ReferenceMonth must be 1-12.");
        if (year < 2020)
            throw new ArgumentException("ReferenceYear must be >= 2020.");
        ReferenceMonth = month;
        ReferenceYear = year;
    }

    public void SetValue(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative.");
        Value = value;
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime paidAt)
    {
        if (Status == ChargeStatus.Paid)
            throw new InvalidOperationException("Charge is already paid.");
        if (Status == ChargeStatus.Cancelled)
            throw new InvalidOperationException("Cancelled charge cannot be paid.");

        Status = ChargeStatus.Paid;
        PaidAt = paidAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOverdue()
    {
        if (Status != ChargeStatus.Pending)
            return;

        Status = ChargeStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUnpaid()
    {
        if (Status != ChargeStatus.Paid)
            throw new InvalidOperationException("Only paid charges can be marked as unpaid.");

        if (DueDate < DateTime.UtcNow.Date)
        {
            Status = ChargeStatus.Overdue;
        }
        else
        {
            Status = ChargeStatus.Pending;
        }

        PaidAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevertToPending()
    {
        if (Status != ChargeStatus.Paid)
            throw new InvalidOperationException("Only paid charges can be reverted to pending.");

        Status = ChargeStatus.Pending;
        PaidAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ChargeStatus.Paid)
            throw new InvalidOperationException("Paid charge cannot be cancelled.");

        Status = ChargeStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
