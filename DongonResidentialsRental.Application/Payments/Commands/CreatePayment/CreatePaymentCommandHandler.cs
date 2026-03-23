using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, PaymentId>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantRepository _tenantRepository;
    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ITenantRepository tenantRepository)
    {
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
    }
    public async Task<PaymentId> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);

        if (tenant is null)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        var paymentAmount = Money.Create(request.Currency, request.Amount);

        var payment = Payment.Create(
            tenant.TenantId, 
            paymentAmount, 
            request.ReceivedOn, 
            request.PaymentMethod, 
            request.Reference);

        _paymentRepository.Add(payment);

        return payment.PaymentId;
    }
}
