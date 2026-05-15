using Microsoft.EntityFrameworkCore;
using Educore.Core.Entities;

namespace Educore.Identity.Api.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.Slug).IsUnique();
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.Resource, p.Action }).IsUnique();
        });

        modelBuilder.Entity<PermissionGroup>(e => e.HasKey(pg => pg.Id));
        modelBuilder.Entity<UserPermission>(e =>
        {
            e.HasKey(up => up.Id);
            e.HasIndex(up => new { up.UserId, up.PermissionId }).IsUnique();
        });
        modelBuilder.Entity<UserGroup>(e =>
        {
            e.HasKey(ug => ug.Id);
            e.HasIndex(ug => new { ug.UserId, ug.GroupId }).IsUnique();
        });
        modelBuilder.Entity<GroupPermission>(e =>
        {
            e.HasKey(gp => gp.Id);
            e.HasIndex(gp => new { gp.GroupId, gp.PermissionId }).IsUnique();
        });
    }
}
