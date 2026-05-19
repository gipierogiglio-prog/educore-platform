namespace Educore.Core.Entities;

public class Class
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Shift { get; set; } = "morning";
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public Guid OrganizationId { get; set; }
    public string? Room { get; set; }
    public bool Active { get; set; } = true;
}
public class Subject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int Workload { get; set; }
    public Guid OrganizationId { get; set; }
    public bool Active { get; set; } = true;
}
public class TeacherSubject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
}
public class SchoolYear
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "planned";
}
public class Grade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public int Bimester { get; set; }
    public decimal Value { get; set; }
    public decimal? RecoveryValue { get; set; }
    public int SchoolYear { get; set; }
    public Guid OrganizationId { get; set; }
}
public class Attendance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public DateTime Date { get; set; }
    public bool Present { get; set; }
    public string? Justification { get; set; }
    public Guid OrganizationId { get; set; }
}
public class AcademicEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string EventType { get; set; } = "event"; // holiday, exam, activity, event, vacation
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AllDay { get; set; } = false;
    public string? Color { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
