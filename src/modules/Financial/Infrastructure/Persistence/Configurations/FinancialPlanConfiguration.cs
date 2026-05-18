using Giglio.EduCore.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Configurations;

public class FinancialPlanConfiguration : IEntityTypeConfiguration<FinancialPlan>
{
    public void Configure(EntityTypeBuilder<FinancialPlan> builder)
    {
        builder.ToTable("FinancialPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EnrollmentId)
            .IsRequired();

        builder.Property(x => x.BaseValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DueDay)
            .IsRequired();

        builder.Property(x => x.DiscountPercent)
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.StartMonth)
            .IsRequired();

        builder.Property(x => x.StartYear)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.EnrollmentId)
            .HasDatabaseName("IX_FinancialPlans_EnrollmentId");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_FinancialPlans_IsActive");

        builder.HasIndex(x => new { x.EnrollmentId, x.IsActive })
            .HasDatabaseName("IX_FinancialPlans_EnrollmentId_Active");
    }
}
