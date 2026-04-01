using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _dbContext;
    public PaymentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void Add(Payment payment)
    {
        _dbContext.Payments.Add(payment);
    }

    public async Task<Payment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments.
                        FirstOrDefaultAsync(p => 
                            p.PaymentId == paymentId, 
                            cancellationToken);
    }

    public void Remove(Payment payment)
    {
        _dbContext.Payments.Remove(payment);
    }
}
