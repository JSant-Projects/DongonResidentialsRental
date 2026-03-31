using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Invoices;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.InvoiceId);

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

        builder.Property(x => x.InvoiceNumber)
            .HasColumnName("invoice_number")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LeaseId)
            .HasColumnName("lease_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new LeaseId(value));

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(x => x.IssuedOn)
            .HasColumnName("issued_on");

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(x => x.BillingPeriod, billingPeriod =>
        {
            billingPeriod.Property(x => x.From)
                .HasColumnName("from")
                .IsRequired();

            billingPeriod.Property(x => x.To)
                .HasColumnName("to")
                .IsRequired();
        });

        builder.Navigation(x => x.BillingPeriod).IsRequired();

        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.AmountPaid);
        builder.Ignore(x => x.AmountCredited);
        builder.Ignore(x => x.Balance);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CreditAllocations)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.CreditAllocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(x => x.LeaseId);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IssuedOn);
        builder.HasIndex("from", "to");
    }
}
