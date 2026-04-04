using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.CreditNotes;

internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("credit_notes");

        builder.HasKey(x => x.CreditNoteId);

        builder.Property(x => x.CreditNoteId)
            .HasColumnName("credit_note_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new CreditNoteId(value));

        builder.Property(x => x.LeaseId)
            .HasColumnName("lease_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new LeaseId(value));

        builder.Property(x => x.IssuedOn)
            .HasColumnName("issued_on");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

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

        builder.Ignore(x => x.AmountApplied);
        builder.Ignore(x => x.RemainingAmount);

        builder.HasOne<Lease>()
            .WithMany()
            .HasForeignKey(x => x.LeaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.CreditNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.LeaseId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IssuedOn);
    }
}
