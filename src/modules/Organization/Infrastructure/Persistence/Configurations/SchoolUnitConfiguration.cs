using Giglio.EduCore.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Organization.Infrastructure.Persistence.Configurations;

public class SchoolUnitConfiguration : IEntityTypeConfiguration<SchoolUnit>
{
    public void Configure(EntityTypeBuilder<SchoolUnit> builder)
    {
        builder.ToTable("SchoolUnits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Neighborhood)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(x => x.ZipCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.ResponsibleName)
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_SchoolUnits_Name");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_SchoolUnits_IsActive");

        builder.HasIndex(x => new { x.City, x.State })
            .HasDatabaseName("IX_SchoolUnits_City_State");
    }
}