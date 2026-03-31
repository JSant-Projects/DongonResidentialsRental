using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Invoices;

internal sealed class InvoiceCreditAllocationConfiguration : IEntityTypeConfiguration<InvoiceCreditAllocation>
{
    public void Configure(EntityTypeBuilder<InvoiceCreditAllocation> builder)
    {
        builder.ToTable("invoice_credit_allocations");

        builder.HasKey(x => x.InvoiceCreditAllocationId);

        builder.Property(x => x.InvoiceCreditAllocationId)
            .HasColumnName("invoice_credit_allocation_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new InvoiceCreditAllocationId(value));

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

        builder.Property(x => x.CreditNoteId)
            .HasColumnName("credit_note_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new CreditNoteId(value));

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
        builder.HasIndex(x => x.CreditNoteId);
    }
}
