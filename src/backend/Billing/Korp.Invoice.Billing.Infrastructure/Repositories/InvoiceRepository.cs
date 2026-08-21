using System.Linq.Expressions;
using Korp.Invoice.Billing.Domain.Entities;
using Korp.Invoice.Billing.Domain.Enums;
using Korp.Invoice.Billing.Domain.Interfaces;
using Korp.Invoice.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Billing.Infrastructure.Repositories;

public sealed class InvoiceRepository : BaseRepository<FiscalInvoice>, IInvoiceRepository
{
    public InvoiceRepository(BillingDbContext context) : base(context) { }
    public override async Task<FiscalInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await DbSet.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<FiscalInvoice?> GetByNumberAsync(long number, CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Number == number, cancellationToken);

    public Task<(IReadOnlyCollection<FiscalInvoice> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        Expression<Func<FiscalInvoice, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (long.TryParse(term, out var number))
                predicate = x => x.Number == number;

        }
        Func<IQueryable<FiscalInvoice>, IOrderedQueryable<FiscalInvoice>> orderBy = (sortBy.ToLowerInvariant(), sortDirection.ToLowerInvariant()) switch
        {
            ("number", "asc") => query => query.OrderBy(x => x.Number),
            ("number", "desc") => query => query.OrderByDescending(x => x.Number),
            ("status", "asc") => query => query.OrderBy(x => x.Status),
            ("status", "desc") => query => query.OrderByDescending(x => x.Status),
            _ => query => query.OrderByDescending(x => x.Number)
        };
        return GetPagedAsync(page, pageSize, predicate, orderBy, cancellationToken);
    }

    public async Task<bool> HasProductAsync(Guid productId, CancellationToken cancellationToken = default)
    => await Context.Set<InvoiceItem>().AsNoTracking().AnyAsync(x => x.ProductId == productId, cancellationToken);

    public async Task<int> CountByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().CountAsync(invoice => invoice.Status == status, cancellationToken);

    public async Task<int> GetProcessedItemsCountAsync(CancellationToken cancellationToken = default)
    => await DbSet.AsNoTracking().Where(invoice => invoice.Status == InvoiceStatus.Closed).SelectMany(invoice => invoice.Items).SumAsync(item => item.Quantity, cancellationToken);

    public async Task<IReadOnlyCollection<(DateTime Date, int Quantity)>> GetDailyConsumptionAsync(int days, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var data = await DbSet
            .AsNoTracking()
            .Where(invoice => invoice.Status == InvoiceStatus.Closed
                && invoice.ClosedAtUtc.HasValue
                && invoice.ClosedAtUtc.Value >= startDate)
            .SelectMany(invoice => invoice.Items
            .Select(item => new
            {
                invoice.ClosedAtUtc!.Value.Date,
                item.Quantity
            })).GroupBy(item => item.Date).Select(group => new
            {
                Date = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            }).OrderBy(item => item.Date).ToListAsync(cancellationToken);

        return [.. data.Select(item => (item.Date, item.Quantity))];
    }

    public async Task<IReadOnlyCollection<(Guid ProductId, int Quantity)>> GetTopProductsAsync(int take, CancellationToken cancellationToken = default)
    {
        var data = await DbSet
            .AsNoTracking()
            .Where(invoice => invoice.Status == InvoiceStatus.Closed)
            .SelectMany(invoice => invoice.Items)
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .OrderByDescending(item => item.Quantity)
            .Take(take)
            .ToListAsync(cancellationToken);

        return [.. data.Select(item => (item.ProductId, item.Quantity))];
    }
}
