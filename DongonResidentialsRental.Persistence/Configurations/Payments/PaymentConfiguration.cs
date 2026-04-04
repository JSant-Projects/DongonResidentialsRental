using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.PaymentId);

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new PaymentId(value));

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new TenantId(value));

        builder.Property(x => x.ReceivedOn)
            .HasColumnName("received_on")
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasColumnName("reference")
            .HasMaxLength(100);

        builder.Property(x => x.Method)
            .HasColumnName("method")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.ReversedOn)
            .HasColumnName("reversed_on");

        builder.Property(x => x.ReversalReason)
            .HasColumnName("reversal_reason")
            .HasMaxLength(500);

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(x => x.Currency)
                .HasColumnName("amount_currency")
                .HasMaxLength(3)
                .IsRequired();

            money.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Navigation(x => x.Amount)
            .IsRequired();

        builder.Ignore(x => x.AllocatedAmount);
        builder.Ignore(x => x.RemainingAmount);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ReceivedOn);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Method);
    }
}
