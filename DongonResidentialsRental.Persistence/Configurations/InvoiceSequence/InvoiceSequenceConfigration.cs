using DongonResidentialsRental.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Models;

internal sealed class InvoiceSequenceConfigration : IEntityTypeConfiguration<InvoiceSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceSequence> builder)
    {

        builder.ToTable("invoice_sequences");

        builder.HasKey(x => x.Year);

        builder.Property(x => x.Year)
               .HasColumnName("year")
               .ValueGeneratedNever();

        builder.Property(x => x.LastNumber)
               .HasColumnName("last_number")
               .IsRequired();
    }
}
