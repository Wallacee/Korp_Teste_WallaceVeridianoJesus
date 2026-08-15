namespace Korp.Invoice.Inventory.Application.DTOs;

public sealed record ProductDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stock { get; init; }
}
