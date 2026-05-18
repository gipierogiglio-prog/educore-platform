using Giglio.EduCore.Academic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Academic.Infrastructure.Persistence.Configurations;

public class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.ToTable("Series");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CourseId)
            .IsRequired();

        builder.Property(x => x.AcademicYear)
            .IsRequired();

        builder.Property(x => x.TotalHours);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationships
        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CourseId)
            .HasDatabaseName("IX_Series_CourseId");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Series_IsActive");

        builder.HasIndex(x => new { x.CourseId, x.AcademicYear })
            .HasDatabaseName("IX_Series_CourseId_AcademicYear");
    }
}