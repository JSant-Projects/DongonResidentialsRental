using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.CreditNote;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class CreditNoteRepository : ICreditNoteRepository
{
    private readonly ApplicationDbContext _dbContext;
    public CreditNoteRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void Add(CreditNote creditNote)
    {
        _dbContext.CreditNotes.Add(creditNote);
    }

    public async Task<CreditNote?> GetByIdAsync(CreditNoteId creditNoteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CreditNotes
                        .FirstOrDefaultAsync(
                            c => c.CreditNoteId == creditNoteId, 
                            cancellationToken);
    }

    public void Remove(CreditNote creditNote)
    {
        _dbContext.CreditNotes.Remove(creditNote);
    }
}
