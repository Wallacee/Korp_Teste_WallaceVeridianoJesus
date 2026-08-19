using Korp.Invoice.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Korp.Invoice.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("StockOperations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();
        builder.HasIndex(x => x.OperationId).IsUnique();
    }
}
