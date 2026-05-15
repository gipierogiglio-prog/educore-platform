using Microsoft.EntityFrameworkCore;
using Educore.Core.Entities;

namespace Educore.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity / Users
    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    // Secretary
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    // Academic
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<SchoolYear> SchoolYears => Set<SchoolYear>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    // Grading
    public DbSet<GradeRule> GradeRules => Set<GradeRule>();
    public DbSet<GradeRuleComponent> GradeRuleComponents => Set<GradeRuleComponent>();
    public DbSet<GradeEntry> GradeEntries => Set<GradeEntry>();
    public DbSet<GradeResult> GradeResults => Set<GradeResult>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Email).IsUnique(); });
        mb.Entity<Organization>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Slug).IsUnique(); });
        mb.Entity<Permission>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.Resource, x.Action }).IsUnique(); });
        mb.Entity<PermissionGroup>(e => e.HasKey(x => x.Id));
        mb.Entity<UserPermission>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.UserId, x.PermissionId }).IsUnique(); });
        mb.Entity<UserGroup>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.UserId, x.GroupId }).IsUnique(); });
        mb.Entity<GroupPermission>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.GroupId, x.PermissionId }).IsUnique(); });
        mb.Entity<Student>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Enrollment).IsUnique(); });
        mb.Entity<Guardian>(e => e.HasKey(x => x.Id));
        mb.Entity<Teacher>(e => e.HasKey(x => x.Id));
        mb.Entity<Enrollment>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.StudentId, x.ClassId, x.SchoolYear }).IsUnique(); });
        mb.Entity<Class>(e => e.HasKey(x => x.Id));
        mb.Entity<Subject>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Code).IsUnique(); });
        mb.Entity<TeacherSubject>(e => e.HasKey(x => x.Id));
        mb.Entity<SchoolYear>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Year).IsUnique(); });
        mb.Entity<Grade>(e => e.HasKey(x => x.Id));
        mb.Entity<Attendance>(e => e.HasKey(x => x.Id));
        mb.Entity<GradeRule>(e => e.HasKey(x => x.Id));
        mb.Entity<GradeRuleComponent>(e => e.HasKey(x => x.Id));
        mb.Entity<GradeEntry>(e => e.HasKey(x => x.Id));
        mb.Entity<GradeResult>(e => e.HasKey(x => x.Id));
    }
}
