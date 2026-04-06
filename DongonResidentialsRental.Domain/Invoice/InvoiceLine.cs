using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class InvoiceLine
{
    public InvoiceLineId InvoiceLineId { get; }
    public InvoiceId InvoiceId { get; }
    public string Description { get; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);
    public InvoiceLineType Type { get; }
    private InvoiceLine() { }
    private InvoiceLine(
        InvoiceLineId invoiceLineId,
        InvoiceId invoiceId, 
        string description, 
        int quantity, 
        Money unitPrice, 
        InvoiceLineType type)
    {
        InvoiceLineId = invoiceLineId;
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

        return new InvoiceLine(
            new InvoiceLineId(Guid.NewGuid()),
            invoiceId, 
            description, 
            quantity, 
            unitPrice, 
            type);
    }

    public void IncreaseQuantity(int quantity)
    {
        Ensure.InRangeInteger(quantity, 1, int.MaxValue, "Amount to increase must be at least 1.");
        Quantity += quantity;
    }


}
