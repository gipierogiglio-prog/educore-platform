using Giglio.EduCore.Disciplinary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Disciplinary.Infrastructure.Persistence;

public class DisciplinaryDbContext : DbContext
{
    public DisciplinaryDbContext(DbContextOptions<DisciplinaryDbContext> options) : base(options) { }

    public DbSet<DisciplinaryIncident> DisciplinaryIncidents => Set<DisciplinaryIncident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DisciplinaryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
