using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Application.Payments.Commands.ReversePayment;

public sealed class ReversePaymentCommandHandler : ICommandHandler<ReversePaymentCommand, Unit>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ReversePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(ReversePaymentCommand request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;

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

        payment.Reverse(today, request.Reason);

        invoice.RemoveAllocation(payment.PaymentId);

        return Unit.Value;
    }
}
