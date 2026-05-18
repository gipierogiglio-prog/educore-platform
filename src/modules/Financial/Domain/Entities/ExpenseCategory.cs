namespace Giglio.EduCore.Financial.Domain.Entities;

public class ExpenseCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Expense> Expenses { get; private set; } = new List<Expense>();

    private ExpenseCategory() { }

    public ExpenseCategory(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        Description = description;
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (name.Length < 2 || name.Length > 100)
            throw new ArgumentException("Name must be between 2 and 100 characters", nameof(name));
        Name = name.Trim();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}
