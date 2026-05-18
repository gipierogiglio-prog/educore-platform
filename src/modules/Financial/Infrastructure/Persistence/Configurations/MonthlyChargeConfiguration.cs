using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Configurations;

public class MonthlyChargeConfiguration : IEntityTypeConfiguration<MonthlyCharge>
{
    public void Configure(EntityTypeBuilder<MonthlyCharge> builder)
    {
        builder.ToTable("MonthlyCharges");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FinancialPlanId)
            .IsRequired();

        builder.Property(x => x.ReferenceMonth)
            .IsRequired();

        builder.Property(x => x.ReferenceYear)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DueDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ChargeStatus.Pending);

        builder.Property(x => x.PaidAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationship
        builder.HasOne(x => x.FinancialPlan)
            .WithMany()
            .HasForeignKey(x => x.FinancialPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.FinancialPlanId, x.ReferenceMonth, x.ReferenceYear })
            .IsUnique()
            .HasDatabaseName("IX_MonthlyCharges_Plan_Month_Year");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_MonthlyCharges_Status");

        builder.HasIndex(x => x.DueDate)
            .HasDatabaseName("IX_MonthlyCharges_DueDate");

        builder.HasIndex(x => new { x.Status, x.DueDate })
            .HasDatabaseName("IX_MonthlyCharges_Status_DueDate");
    }
}
