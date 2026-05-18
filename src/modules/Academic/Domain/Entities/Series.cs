namespace Giglio.EduCore.Academic.Domain.Entities;

public class Series
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid CourseId { get; private set; }
    public int AcademicYear { get; private set; }
    public int? TotalHours { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property (EF Core)
    public Course? Course { get; private set; }
    public ICollection<CurriculumMatrix> CurriculumEntries { get; private set; } = new List<CurriculumMatrix>();

    private Series() { }

    public Series(string name, Guid courseId, int academicYear, int? totalHours = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetCourse(courseId);
        SetAcademicYear(academicYear);
        SetTotalHours(totalHours);
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

    public void SetCourse(Guid courseId)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId cannot be empty", nameof(courseId));
        CourseId = courseId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAcademicYear(int academicYear)
    {
        if (academicYear < 1900 || academicYear > 2100)
            throw new ArgumentException("Academic year must be between 1900 and 2100", nameof(academicYear));
        AcademicYear = academicYear;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTotalHours(int? totalHours)
    {
        if (totalHours.HasValue && totalHours.Value <= 0)
            throw new ArgumentException("Total hours must be greater than zero", nameof(totalHours));
        TotalHours = totalHours;
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