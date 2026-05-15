namespace Educore.Core.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Enrollment { get; set; } = "";
    public Guid UserId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? GuardianId { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}
public class Guardian
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Relationship { get; set; } = "";
}
public class Teacher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? Specialization { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}
public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public int SchoolYear { get; set; }
    public string Status { get; set; } = "active";
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}
