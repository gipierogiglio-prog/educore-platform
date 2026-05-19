using Giglio.EduCore.DailyRoutines.Domain.Enums;

namespace Giglio.EduCore.DailyRoutines.Domain.Entities;

public class DailyRoutineRecord
{
    public Guid Id { get; private set; }
    public Guid ClassRoutineId { get; private set; }
    public DateTime RecordDate { get; private set; }
    public TimeSpan? StartTime { get; private set; }
    public TimeSpan? EndTime { get; private set; }
    public RoutineRecordStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid? TeacherId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ClassRoutine? ClassRoutine { get; private set; }

    private DailyRoutineRecord() { }

    public DailyRoutineRecord(Guid classRoutineId, DateTime recordDate)
    {
        Id = Guid.NewGuid();
        SetClassRoutine(classRoutineId);
        SetRecordDate(recordDate);
        Status = RoutineRecordStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetClassRoutine(Guid classRoutineId)
    {
        if (classRoutineId == Guid.Empty)
            throw new ArgumentException("ClassRoutineId cannot be empty", nameof(classRoutineId));
        ClassRoutineId = classRoutineId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRecordDate(DateTime recordDate)
    {
        RecordDate = recordDate.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(TimeSpan startTime, Guid? teacherId = null)
    {
        StartTime = startTime;
        TeacherId = teacherId;
        Status = RoutineRecordStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(TimeSpan? endTime = null, string? notes = null)
    {
        EndTime = endTime ?? DateTime.UtcNow.TimeOfDay;
        Notes = notes;
        Status = RoutineRecordStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        Notes = reason;
        Status = RoutineRecordStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(RoutineRecordStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
