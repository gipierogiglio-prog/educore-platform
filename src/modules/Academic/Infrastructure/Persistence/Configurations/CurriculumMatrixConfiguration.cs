using Giglio.EduCore.Academic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Academic.Infrastructure.Persistence.Configurations;

public class CurriculumMatrixConfiguration : IEntityTypeConfiguration<CurriculumMatrix>
{
    public void Configure(EntityTypeBuilder<CurriculumMatrix> builder)
    {
        builder.ToTable("CurriculumMatrices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeriesId)
            .IsRequired();

        builder.Property(x => x.SubjectId)
            .IsRequired();

        builder.Property(x => x.WeeklyHours)
            .IsRequired();

        builder.Property(x => x.TotalHours);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Series)
            .WithMany(x => x.CurriculumEntries)
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.SeriesId)
            .HasDatabaseName("IX_CurriculumMatrices_SeriesId");

        builder.HasIndex(x => x.SubjectId)
            .HasDatabaseName("IX_CurriculumMatrices_SubjectId");

        builder.HasIndex(x => new { x.SeriesId, x.SubjectId })
            .IsUnique()
            .HasDatabaseName("IX_CurriculumMatrices_SeriesId_SubjectId");
    }
}