using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Exceptions;
namespace Korp.Invoice.Billing.Domain.Entities;

public sealed class FiscalInvoice
{
    private readonly List<InvoiceItem> _items = [];
    public Guid Id{get;private set;} = Guid.NewGuid();
    public long Number{get;private set;}
    public InvoiceStatus Status{get;private set;}
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    protected FiscalInvoice() { }
    public FiscalInvoice(long number)
    {
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number), "O número da nota fiscal deve ser maior que zero.");
        Number = number;
        Status = InvoiceStatus.Open;
    }
    public void AddItem(Guid productId, int quantity)
    {
        EnsureOpen();
        if (_items.Any(x => x.ProductId == productId)) throw new DuplicateInvoiceItemException(productId);
        _items.Add(new InvoiceItem(Id, productId, quantity));
    }
    public void Close()
    {
        EnsureOpen();
        if (_items.Count == 0) throw new InvoiceWithoutItemsException();
        Status = InvoiceStatus.Closed;
    }
    private void EnsureOpen()
    {
        if (Status == InvoiceStatus.Closed) throw new InvoiceAlreadyClosedException();
    }
}
