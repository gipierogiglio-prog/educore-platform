using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence;

public class DailyRoutinesDbContext : DbContext
{
    public DailyRoutinesDbContext(DbContextOptions<DailyRoutinesDbContext> options) : base(options) { }

    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<ClassRoutine> ClassRoutines => Set<ClassRoutine>();
    public DbSet<DailyRoutineRecord> DailyRoutineRecords => Set<DailyRoutineRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DailyRoutinesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
