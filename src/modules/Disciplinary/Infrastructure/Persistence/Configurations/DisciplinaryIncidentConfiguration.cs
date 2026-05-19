using Giglio.EduCore.Disciplinary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Disciplinary.Infrastructure.Persistence.Configurations;

public class DisciplinaryIncidentConfiguration : IEntityTypeConfiguration<DisciplinaryIncident>
{
    public void Configure(EntityTypeBuilder<DisciplinaryIncident> builder)
    {
        builder.ToTable("DisciplinaryIncidents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.RecordedById)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("pending");

        builder.Property(x => x.Resolution)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.StudentId)
            .HasDatabaseName("IX_DisciplinaryIncidents_StudentId");

        builder.HasIndex(x => x.ClassId)
            .HasDatabaseName("IX_DisciplinaryIncidents_ClassId");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_DisciplinaryIncidents_Type");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_DisciplinaryIncidents_Status");

        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("IX_DisciplinaryIncidents_OccurredAt");

        builder.HasIndex(x => new { x.ClassId, x.Type, x.OccurredAt })
            .HasDatabaseName("IX_DisciplinaryIncidents_ClassId_Type_Date");
    }
}
