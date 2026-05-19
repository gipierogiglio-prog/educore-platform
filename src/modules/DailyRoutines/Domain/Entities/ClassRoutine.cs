using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Entities;

public class ClassRoutine
{
    public Guid Id { get; private set; }
    public Guid ClassId { get; private set; }
    public Guid RoutineId { get; private set; }
    public WeekDay WeekDay { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Routine? Routine { get; private set; }
    public ICollection<DailyRoutineRecord> Records { get; private set; } = new List<DailyRoutineRecord>();

    private ClassRoutine() { }

    public ClassRoutine(Guid classId, Guid routineId, WeekDay weekDay, TimeSpan startTime, int durationMinutes)
    {
        Id = Guid.NewGuid();
        SetClass(classId);
        SetRoutine(routineId);
        SetWeekDay(weekDay);
        SetStartTime(startTime);
        SetDuration(durationMinutes);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetClass(Guid classId)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("ClassId cannot be empty", nameof(classId));
        ClassId = classId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRoutine(Guid routineId)
    {
        if (routineId == Guid.Empty)
            throw new ArgumentException("RoutineId cannot be empty", nameof(routineId));
        RoutineId = routineId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWeekDay(WeekDay weekDay)
    {
        WeekDay = weekDay;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStartTime(TimeSpan startTime)
    {
        StartTime = startTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDuration(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Duration must be greater than zero", nameof(minutes));
        DurationMinutes = minutes;
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
