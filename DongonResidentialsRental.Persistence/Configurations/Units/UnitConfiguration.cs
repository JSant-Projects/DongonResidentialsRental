using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Units;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units");

        builder.HasKey(x => x.UnitId);

        builder.Property(x => x.UnitId)
            .HasColumnName("unit_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new UnitId(value));

        builder.Property(x => x.BuildingId)
            .HasColumnName("building_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new BuildingId(value));

        builder.Property(x => x.UnitNumber)
            .HasColumnName("unit_number")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Floor)
            .HasColumnName("floor");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.BuildingId, x.UnitNumber })
            .IsUnique();
    }
}
