using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Data;

public interface IApplicationDBContext
{
    DbSet<Building> Buildings { get; }
    DbSet<Unit> Units { get; }
    DbSet<Lease> Leases { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Tenant> Tenants { get; }
}
