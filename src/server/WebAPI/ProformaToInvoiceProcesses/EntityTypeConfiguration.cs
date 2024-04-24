using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.ProformaToInvoiceProcesses;

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaToInvoiceProcess>
{
    public void Configure(EntityTypeBuilder<ProformaToInvoiceProcess> builder)
    {
        builder
            .ToTable(Tables.ProformaToInvoiceProcesses);

        builder
            .HasKey(field => field.InvoiceId);

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(pti => pti.InvoiceId);
    }
}

public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaToInvoiceProcessItem>
{
    public void Configure(EntityTypeBuilder<ProformaToInvoiceProcessItem> builder)
    {
        builder
            .ToTable(Tables.ProformaToInvoiceProcessItems);

        builder
            .HasKey(field => new { field.InvoiceId, field.ProformaId });
    }
}