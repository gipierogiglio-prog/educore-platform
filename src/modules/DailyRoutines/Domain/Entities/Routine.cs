using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Entities;

public class Routine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public RoutineCategory Category { get; private set; }
    public int ExpectedDurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ICollection<ClassRoutine> ClassRoutines { get; private set; } = new List<ClassRoutine>();

    private Routine() { }

    public Routine(string name, RoutineCategory category, int expectedDurationMinutes, string? description = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetCategory(category);
        SetExpectedDuration(expectedDurationMinutes);
        SetDescription(description);
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

    public void SetCategory(RoutineCategory category)
    {
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpectedDuration(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Expected duration must be greater than zero", nameof(minutes));
        ExpectedDurationMinutes = minutes;
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
