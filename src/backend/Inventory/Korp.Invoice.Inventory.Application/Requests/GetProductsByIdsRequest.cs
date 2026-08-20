namespace Korp.Invoice.Inventory.Application.Requests;

public sealed class GetProductsByIdsRequest
{
    public IReadOnlyCollection<Guid> Ids { get; init; } = [];
}
