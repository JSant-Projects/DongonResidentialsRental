using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Buildings;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("buildings");

        builder.HasKey(x => x.BuildingId);

        builder.Property(x => x.BuildingId)
            .HasColumnName("building_id")
            .HasConversion(
                id => id.Id, 
                value => new BuildingId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Street)
                .HasColumnName("street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.Province)
                .HasColumnName("province")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
