namespace Giglio.EduCore.Disciplinary.Domain.Entities;

public class DisciplinaryIncident
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid ClassId { get; private set; }
    public string Type { get; private set; }
    public string Description { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid RecordedById { get; private set; }
    public string Status { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private DisciplinaryIncident() { }

    public DisciplinaryIncident(
        Guid studentId,
        Guid classId,
        string type,
        string description,
        Guid recordedById,
        DateTime occurredAt)
    {
        Id = Guid.NewGuid();
        SetStudent(studentId);
        SetClass(classId);
        SetType(type);
        SetDescription(description);
        SetRecordedBy(recordedById);
        SetOccurredAt(occurredAt);
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
    }

    public void SetStudent(Guid studentId)
    {
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId cannot be empty", nameof(studentId));
        StudentId = studentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetClass(Guid classId)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("ClassId cannot be empty", nameof(classId));
        ClassId = classId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetType(string type)
    {
        var allowed = new[] { "warning", "suspension", "occurrence", "expulsion", "other" };
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required", nameof(type));
        if (!allowed.Contains(type.Trim().ToLowerInvariant()))
            throw new ArgumentException($"Type must be one of: {string.Join(", ", allowed)}", nameof(type));
        Type = type.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
        Description = description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRecordedBy(Guid recordedById)
    {
        if (recordedById == Guid.Empty)
            throw new ArgumentException("RecordedById cannot be empty", nameof(recordedById));
        RecordedById = recordedById;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOccurredAt(DateTime occurredAt)
    {
        if (occurredAt > DateTime.UtcNow.AddDays(1))
            throw new ArgumentException("OccurredAt cannot be in the future", nameof(occurredAt));
        OccurredAt = occurredAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(string status)
    {
        var allowed = new[] { "pending", "resolved", "dismissed" };
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));
        if (!allowed.Contains(status.Trim().ToLowerInvariant()))
            throw new ArgumentException($"Status must be one of: {string.Join(", ", allowed)}", nameof(status));
        Status = status.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResolution(string? resolution)
    {
        Resolution = resolution?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
