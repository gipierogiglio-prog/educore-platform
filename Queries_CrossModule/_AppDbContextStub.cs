using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Financial.Application.Queries;

/// <summary>
/// Stub para compilação. O AppDbContext real está em outro módulo.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<object> Classes => Set<object>();
    public DbSet<object> Enrollments => Set<object>();
    public DbSet<object> Students => Set<object>();
    public DbSet<object> Users => Set<object>();
}
