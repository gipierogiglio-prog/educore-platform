using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Giglio.EduCore.Financial.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PaymentDate)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Observation)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.CreatedByUserName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CancelledByUserName)
            .HasMaxLength(200);

        builder.Property(x => x.CancelReason)
            .HasMaxLength(500);

        // Relationship
        builder.HasOne(x => x.MonthlyCharge)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.MonthlyChargeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.MonthlyChargeId)
            .HasDatabaseName("IX_Payments_MonthlyChargeId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Payments_CreatedAt");

        builder.HasIndex(x => new { x.MonthlyChargeId, x.CancelledAt })
            .HasDatabaseName("IX_Payments_MonthlyChargeId_Active");
    }
}
