using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DongonResidentialsRental.Persistence.Configurations.Payments;

internal sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("payment_allocations");

        builder.HasKey(x => x.PaymentAllocationId);

        builder.Property(x => x.PaymentAllocationId)
            .HasColumnName("payment_allocation_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new PaymentAllocationId(value));

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new PaymentId(value));

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

        builder.Property(x => x.AllocatedOn)
            .HasColumnName("allocated_on")
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

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.InvoiceId);
    }
}
