namespace Korp.Invoice.Billing.Domain.Entities;

public sealed class InvoiceItem
{
    public Guid Id{get;private set;} = Guid.NewGuid();
    public Guid InvoiceId{get;private set;}
    public Guid ProductId{get;private set;}
    public int Quantity{get;private set;}
    protected InvoiceItem() { }
    internal InvoiceItem(Guid invoiceId, Guid productId, int quantity)
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("O identificador da nota fiscal é obrigatório.", nameof(invoiceId));

        if (productId == Guid.Empty)
            throw new ArgumentException("O identificador do produto é obrigatório.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero.");

        InvoiceId = invoiceId;
        ProductId = productId;
        Quantity = quantity;
    }
}
