using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DongonResidentialsRental.Persistence.Configurations.CreditNotes;

internal sealed class CreditAllocationConfiguration : IEntityTypeConfiguration<CreditAllocation>
{
    public void Configure(EntityTypeBuilder<CreditAllocation> builder)
    {
        builder.ToTable("credit_allocations");

        builder.HasKey(x => x.CreditAllocationId);

        builder.Property(x => x.CreditAllocationId)
            .HasColumnName("credit_allocation_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => new CreditAllocationId(value));

        builder.Property(x => x.CreditNoteId)
            .HasColumnName("credit_note_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new CreditNoteId(value));

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

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

        builder.Navigation(x => x.Amount)
            .IsRequired();

        builder.HasIndex(x => x.CreditNoteId);
        builder.HasIndex(x => x.InvoiceId);
    }
}
