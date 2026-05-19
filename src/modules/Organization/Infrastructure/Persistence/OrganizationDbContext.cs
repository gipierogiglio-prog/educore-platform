using Giglio.EduCore.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giglio.EduCore.Organization.Infrastructure.Persistence;

public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : base(options) { }

    public DbSet<SchoolUnit> SchoolUnits => Set<SchoolUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}