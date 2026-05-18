namespace Giglio.EduCore.Academic.Domain.Entities;

public class CurriculumMatrix
{
    public Guid Id { get; private set; }
    public Guid SeriesId { get; private set; }
    public Guid SubjectId { get; private set; }
    public int WeeklyHours { get; private set; }
    public int? TotalHours { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties (EF Core)
    public Series? Series { get; private set; }

    private CurriculumMatrix() { }

    public CurriculumMatrix(Guid seriesId, Guid subjectId, int weeklyHours, int? totalHours = null)
    {
        Id = Guid.NewGuid();
        SetSeries(seriesId);
        SetSubject(subjectId);
        SetWeeklyHours(weeklyHours);
        SetTotalHours(totalHours);
        CreatedAt = DateTime.UtcNow;
    }

    public void SetSeries(Guid seriesId)
    {
        if (seriesId == Guid.Empty)
            throw new ArgumentException("SeriesId cannot be empty", nameof(seriesId));
        SeriesId = seriesId;
    }

    public void SetSubject(Guid subjectId)
    {
        if (subjectId == Guid.Empty)
            throw new ArgumentException("SubjectId cannot be empty", nameof(subjectId));
        SubjectId = subjectId;
    }

    public void SetWeeklyHours(int weeklyHours)
    {
        if (weeklyHours <= 0)
            throw new ArgumentException("Weekly hours must be greater than zero", nameof(weeklyHours));
        if (weeklyHours > 40)
            throw new ArgumentException("Weekly hours cannot exceed 40", nameof(weeklyHours));
        WeeklyHours = weeklyHours;
    }

    public void SetTotalHours(int? totalHours)
    {
        if (totalHours.HasValue && totalHours.Value <= 0)
            throw new ArgumentException("Total hours must be greater than zero", nameof(totalHours));
        TotalHours = totalHours;
    }
}