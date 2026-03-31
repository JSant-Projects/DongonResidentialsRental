using DongonResidentialsRental.Domain.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Invoices;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(x => x.InvoiceLineId);

        builder.Property(x => x.InvoiceLineId)
            .HasColumnName("invoice_line_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new InvoiceLineId(value));

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new InvoiceId(value));

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.OwnsOne(x => x.UnitPrice, money =>
        {
            money.Property(x => x.Currency)
                .HasColumnName("unit_price_currency")
                .HasMaxLength(3)
                .IsRequired();

            money.Property(x => x.Amount)
                .HasColumnName("unit_price_amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Navigation(x => x.UnitPrice).IsRequired();

        builder.Ignore(x => x.LineTotal);

        builder.HasIndex(x => x.InvoiceId);
    }
}
