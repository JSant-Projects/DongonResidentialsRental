using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Configurations.Leases;

internal sealed class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.ToTable("leases");

        builder.HasKey(x => x.LeaseId);

        builder.Property(x => x.LeaseId)
            .HasColumnName("lease_id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Id,
                value => new LeaseId(value));

        builder.Property(x => x.Occupancy)
            .HasColumnName("tenant_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new TenantId(value));

        builder.Property(x => x.UnitId)
            .HasColumnName("unit_id")
            .IsRequired()
            .HasConversion(
                id => id.Id,
                value => new UnitId(value));

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(x => x.MonthlyRate, money =>
        {
            money.Property(x => x.Currency)
                .HasColumnName("monthly_rate_currency")
                .HasMaxLength(3)
                .IsRequired();

            money.Property(x => x.Amount)
                .HasColumnName("monthly_rate_amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Term, term =>
        {
            term.Property(x => x.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            term.Property(x => x.EndDate)
                .HasColumnName("end_date");

            term.HasIndex(x => new { x.StartDate, x.EndDate });
        });

        builder.OwnsOne(x => x.BillingSettings, billing =>
        {
            billing.Property(x => x.DueDayOfMonth)
                .HasColumnName("due_day_of_month")
                .IsRequired();

            billing.Property(x => x.GracePeriodDays)
                .HasColumnName("grace_period_days")
                .IsRequired();
        });

        builder.OwnsOne(x => x.UtilityResponsibility, utility =>
        {
            utility.Property(x => x.TenantPaysElectricity)
                .HasColumnName("tenant_pays_electricity")
                .IsRequired();

            utility.Property(x => x.TenantPaysWater)
                .HasColumnName("tenant_pays_water")
                .IsRequired();
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.Occupancy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.MonthlyRate).IsRequired();
        builder.Navigation(x => x.Term).IsRequired();
        builder.Navigation(x => x.BillingSettings).IsRequired();
        builder.Navigation(x => x.UtilityResponsibility).IsRequired();

        builder.HasIndex(x => x.Occupancy);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.Status);
    }
}
