namespace Giglio.EduCore.Academic.Domain.Entities;

public class Course
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int TotalHours { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Course() { }

    public Course(string name, int totalHours, string? description = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetTotalHours(totalHours);
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTotalHours(int totalHours)
    {
        if (totalHours <= 0)
            throw new ArgumentException("Total hours must be greater than zero", nameof(totalHours));
        TotalHours = totalHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}