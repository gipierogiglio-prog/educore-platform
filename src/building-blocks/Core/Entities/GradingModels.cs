namespace Educore.Core.Entities;

public class GradeRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<GradeRuleComponent> Components { get; set; } = new List<GradeRuleComponent>();
}
public class GradeRuleComponent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GradeRuleId { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "exam";
    public decimal Weight { get; set; } = 1;
    public int MaxValue { get; set; } = 10;
    public int Order { get; set; }
    public bool HasRecovery { get; set; } = false;
}
public class GradeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid GradeRuleComponentId { get; set; }
    public int Bimester { get; set; }
    public int SchoolYear { get; set; }
    public decimal? Value { get; set; }
    public decimal? RecoveryValue { get; set; }
    public Guid OrganizationId { get; set; }
}
public class GradeResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid GradeRuleId { get; set; }
    public Guid SubjectId { get; set; }
    public int Bimester { get; set; }
    public int SchoolYear { get; set; }
    public decimal FinalValue { get; set; }
    public string Status { get; set; } = "";
    public Guid OrganizationId { get; set; }
}
