using System.Linq.Expressions;
using Korp.Invoice.Billing.Domain.Entities;
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

    public Task<(IReadOnlyCollection<FiscalInvoice> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken =
        default)
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
}
