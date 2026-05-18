using Giglio.EduCore.Academic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Academic.Infrastructure.Persistence;

public class AcademicDbContext : DbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<CurriculumMatrix> CurriculumMatrices => Set<CurriculumMatrix>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AcademicDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}