namespace Korp.Invoice.Shared.Pagination;

public sealed class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
