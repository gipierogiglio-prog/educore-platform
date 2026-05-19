using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Configurations;

public class RoutineConfiguration : IEntityTypeConfiguration<Routine>
{
    public void Configure(EntityTypeBuilder<Routine> builder)
    {
        builder.ToTable("Routines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ExpectedDurationMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_Routines_Category");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Routines_IsActive");

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_Routines_Name");
    }
}
