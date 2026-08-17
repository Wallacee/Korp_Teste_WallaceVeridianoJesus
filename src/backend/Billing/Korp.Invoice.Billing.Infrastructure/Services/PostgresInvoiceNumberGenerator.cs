using Korp.Invoice.Billing.Domain.Services;
using Korp.Invoice.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Korp.Invoice.Billing.Infrastructure.Services;

public sealed class PostgresInvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly BillingDbContext _context;
    public PostgresInvoiceNumberGenerator(BillingDbContext context)
    {
        _context = context;
    }
    public async Task<long> GetNextAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using
        var command = connection.CreateCommand();

        command.CommandText = """SELECT nextval('"InvoiceNumberSequence "');""";

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(result);
    }
}
