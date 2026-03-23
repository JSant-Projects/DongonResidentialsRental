using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class InvoiceLine
{
    public InvoiceLineId InvoiceLineId { get; }
    public InvoiceId InvoiceId { get; }
    public string Description { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);
    public InvoiceLineType Type { get; }
    private InvoiceLine() { }
    private InvoiceLine(InvoiceId invoiceId, string description, int quantity, Money unitPrice, InvoiceLineType type)
    {
        InvoiceLineId = new InvoiceLineId(Guid.NewGuid());
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Type = type;
    }

    internal static InvoiceLine Create(
       InvoiceId invoiceId,
       string description,
       int quantity,
       Money unitPrice,
       InvoiceLineType type)
    {
        Ensure.NotNullOrWhiteSpace(description);
        Ensure.InRangeInteger(quantity, 1, int. MaxValue, "Quantity must be at least 1.");
        Ensure.NotNull(unitPrice, "Unit price cannot be null");

        if (type != InvoiceLineType.Adjustment && unitPrice.Amount < 0)
            throw new DomainException("Only Adjustment lines may be negative.");

        if (type == InvoiceLineType.Adjustment && unitPrice.Amount == 0)
            throw new DomainException("Adjustment amount cannot be zero.");

        return new InvoiceLine(invoiceId, description, quantity, unitPrice, type);
    }


}
