using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Payments.Commands.ApplyPaymentToInvoice;

public sealed class ApplyPaymentToInvoiceCommandHandler : ICommandHandler<ApplyPaymentToInvoiceCommand, Unit>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ApplyPaymentToInvoiceCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;   
    }
    public async Task<Unit> Handle(ApplyPaymentToInvoiceCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);

        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);

        if (payment is null)
        {
            throw new NotFoundException(nameof(Payment), request.PaymentId);
        }

        var amount = Money.Create(payment.Amount.Currency, request.Amount);

        payment.ApplyToInvoice(invoice.InvoiceId, amount, today);
        invoice.ApplyPayment(payment.PaymentId, amount, today);

        return Unit.Value;

    }
}
