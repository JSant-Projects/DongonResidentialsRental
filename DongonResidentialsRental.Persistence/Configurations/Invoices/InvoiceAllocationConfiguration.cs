using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Invoices;

internal sealed class InvoiceAllocationConfiguration : IEntityTypeConfiguration<InvoiceAllocation>
{
    public void Configure(EntityTypeBuilder<InvoiceAllocation> builder)
    {
        builder.ToTable("invoice_allocations");

        builder.HasKey(x => x.InvoiceAllocationId);

        builder.Property(x => x.InvoiceAllocationId)
            .HasColumnName("invoice_allocation_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new InvoiceAllocationId(value));

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new PaymentId(value));

        builder.Property(x => x.AppliedOn)
            .HasColumnName("applied_on")
            .IsRequired();

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

        builder.Navigation(x => x.Amount).IsRequired();

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.PaymentId);
    }
}
