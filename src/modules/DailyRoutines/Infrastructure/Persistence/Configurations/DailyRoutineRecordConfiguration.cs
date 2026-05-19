using Giglio.EduCore.DailyRoutines.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.DailyRoutines.Infrastructure.Persistence.Configurations;

public class DailyRoutineRecordConfiguration : IEntityTypeConfiguration<DailyRoutineRecord>
{
    public void Configure(EntityTypeBuilder<DailyRoutineRecord> builder)
    {
        builder.ToTable("DailyRoutineRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClassRoutineId)
            .IsRequired();

        builder.Property(x => x.RecordDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.StartTime);

        builder.Property(x => x.EndTime);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.TeacherId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationships
        builder.HasOne(x => x.ClassRoutine)
            .WithMany(cr => cr.Records)
            .HasForeignKey(x => x.ClassRoutineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ClassRoutineId)
            .HasDatabaseName("IX_DailyRoutineRecords_ClassRoutineId");

        builder.HasIndex(x => x.RecordDate)
            .HasDatabaseName("IX_DailyRoutineRecords_RecordDate");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_DailyRoutineRecords_Status");

        builder.HasIndex(x => new { x.ClassRoutineId, x.RecordDate })
            .IsUnique()
            .HasDatabaseName("IX_DailyRoutineRecords_ClassRoutineId_Date");
    }
}
