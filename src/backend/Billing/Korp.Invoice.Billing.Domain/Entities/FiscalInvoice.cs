using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Exceptions;
namespace Korp.Invoice.Billing.Domain.Entities;

public sealed class FiscalInvoice
{
    private readonly List<InvoiceItem> _items = [];
    public Guid Id { get; private set; } = Guid.NewGuid();
    public long Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    protected FiscalInvoice() { }
    public FiscalInvoice(long number)
    {
        if (number <= 0) throw
                new ArgumentOutOfRangeException(nameof(number), "O número da nota fiscal deve ser maior que zero.");
        Number = number;
        Status = InvoiceStatus.Open;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public void AddItem(Guid productId, int quantity)
    {
        EnsureOpen();

        if (_items.Any(x => x.ProductId == productId))
            throw new DuplicateInvoiceItemException(productId);

        _items.Add(new InvoiceItem(Id, productId, quantity));
    }
    public void Close()
    {
        EnsureCanBeProcessed();

        Status = InvoiceStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
    }
    public void EnsureOpen()
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvoiceAlreadyClosedException();
    }
    public void ReplaceItems(IEnumerable<(Guid ProductId, int Quantity)> items)
    {
        EnsureOpen();

        _items.Clear();

        foreach (var (ProductId, Quantity) in items)
            AddItem(ProductId, Quantity);
    }

    public void EnsureCanBeProcessed()
    {
        EnsureOpen();

        if (_items.Count == 0)
            throw new InvoiceWithoutItemsException();
    }


}
