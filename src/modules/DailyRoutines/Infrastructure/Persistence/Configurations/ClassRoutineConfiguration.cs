using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Configurations;

public class ClassRoutineConfiguration : IEntityTypeConfiguration<ClassRoutine>
{
    public void Configure(EntityTypeBuilder<ClassRoutine> builder)
    {
        builder.ToTable("ClassRoutines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.RoutineId)
            .IsRequired();

        builder.Property(x => x.WeekDay)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationships
        builder.HasOne(x => x.Routine)
            .WithMany(r => r.ClassRoutines)
            .HasForeignKey(x => x.RoutineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ClassId)
            .HasDatabaseName("IX_ClassRoutines_ClassId");

        builder.HasIndex(x => x.RoutineId)
            .HasDatabaseName("IX_ClassRoutines_RoutineId");

        builder.HasIndex(x => new { x.ClassId, x.WeekDay })
            .HasDatabaseName("IX_ClassRoutines_ClassId_WeekDay");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_ClassRoutines_IsActive");
    }
}
