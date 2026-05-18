using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Domain.Entities;

public class Expense
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public ExpenseCategory Category { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public string? ProviderName { get; private set; }
    public decimal Value { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Expense() { }

    public Expense(
        Guid categoryId,
        string description,
        decimal value,
        DateTime dueDate,
        string? providerName = null)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        SetDescription(description);
        SetValue(value);
        SetDueDate(dueDate);
        ProviderName = providerName?.Trim();
        Status = ExpenseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
        if (description.Trim().Length < 3 || description.Trim().Length > 500)
            throw new ArgumentException("Description must be between 3 and 500 characters");
        Description = description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValue(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be greater than zero", nameof(value));
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pay(DateTime paymentDate)
    {
        if (Status == ExpenseStatus.Paid)
            throw new InvalidOperationException("Expense is already paid");
        if (Status == ExpenseStatus.Cancelled)
            throw new InvalidOperationException("Cannot pay a cancelled expense");

        PaymentDate = paymentDate;
        Status = ExpenseStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ExpenseStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid expense");
        if (Status == ExpenseStatus.Cancelled)
            throw new InvalidOperationException("Expense is already cancelled");

        Status = ExpenseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkOverdue()
    {
        if (Status != ExpenseStatus.Pending)
            return;
        Status = ExpenseStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }
}
