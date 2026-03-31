using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Tenants;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(x => x.TenantId);

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new TenantId(value));

        // -------------------------
        // PersonalInfo (VO)
        // -------------------------
        builder.OwnsOne(x => x.PersonalInfo, personal =>
        {
            personal.Property(p => p.FirstName)
                .HasColumnName("first_name")
                .IsRequired()
                .HasMaxLength(100);

            personal.Property(p => p.LastName)
                .HasColumnName("last_name")
                .IsRequired()
                .HasMaxLength(100);

            // FullName is computed → ignore
            personal.Ignore(p => p.FullName);
        });

        // -------------------------
        // ContactInfo (VO)
        // -------------------------
        builder.OwnsOne(x => x.ContactInfo, contact =>
        {
            // Email VO
            contact.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(255);
            });

            // PhoneNumber VO
            contact.OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value)
                    .HasColumnName("phone_number")
                    .IsRequired()
                    .HasMaxLength(20);
            });
        });

        // Optional but recommended index
        builder.HasIndex("email").IsUnique();
    }
}
