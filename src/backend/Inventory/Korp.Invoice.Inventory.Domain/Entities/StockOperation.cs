namespace Korp.Invoice.Inventory.Domain.Entities;

public sealed class StockOperation
{
    public Guid Id{get;private set;} = Guid.NewGuid();
    public Guid OperationId{get;private set;}
    public DateTime ProcessedAtUtc{get;private set;}
    protected StockOperation() { }
    public StockOperation(Guid operationId)
    {
        if (operationId == Guid.Empty)
            throw new ArgumentException("O identificador da operação é obrigatório.", nameof(operationId));
        OperationId = operationId;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
