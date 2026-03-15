using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnits;

public static class UnitQueryExtensions
{
    public static IQueryable<Unit> ApplyStatusFilter(
        this IQueryable<Unit> query,
        UnitStatus? status)
    {
        if (status is null)
            return query;

        return query.Where(u => u.Status == status.Value);
    }

    public static IQueryable<Unit> ApplySearch(
        this IQueryable<Unit> query,
        string? searchTerm,
        int? floor)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            var pattern = $"%{searchTerm}%";

            query = query.Where(u =>
                EF.Functions.Like(u.UnitNumber, pattern));
        }

        if (floor is not null)
        {
            query = query.Where(u => u.Floor == floor);
        }

        return query;
    }

    public static IQueryable<Unit> ApplyOrdering(this IQueryable<Unit> query)
    {
        return query
            .OrderBy(u => u.UnitNumber);
    }
}
