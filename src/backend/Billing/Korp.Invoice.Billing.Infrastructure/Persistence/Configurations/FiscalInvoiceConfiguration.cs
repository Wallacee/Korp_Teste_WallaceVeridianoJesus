using Korp.Invoice.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Korp.Invoice.Billing.Infrastructure.Persistence.Configurations;

public sealed class FiscalInvoiceConfiguration : IEntityTypeConfiguration<FiscalInvoice>
{
    public void Configure(EntityTypeBuilder<FiscalInvoice> builder)
    {
        builder.ToTable("FiscalInvoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Number).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.HasIndex(x => x.Number).IsUnique();
        builder.Property(x => x.CreatedAtUtc).IsRequired().HasColumnType("timestamp with time zone");
        builder.Property(x => x.ClosedAtUtc).IsRequired(false).HasColumnType("timestamp with time zone");
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
