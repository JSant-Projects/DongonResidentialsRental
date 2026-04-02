using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Services;

internal sealed class InvoiceGenerator : IInvoiceNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;
    public InvoiceGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;

        var sequence = await _dbContext.InvoiceSequences
                                .FirstOrDefaultAsync(x => 
                                    x.Year == year, 
                                    cancellationToken);

        if (sequence is null)
        {
            sequence = new InvoiceSequence
            {
                Year = year,
                LastNumber = 0
            };

            _dbContext.InvoiceSequences.Add(sequence);
        }

        sequence.LastNumber++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return $"INV-{year}-{sequence.LastNumber:D6}";
    }
}
