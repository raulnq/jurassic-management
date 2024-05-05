using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.InvoiceToCollectionProcesses;

public class EntityTypeConfiguration : IEntityTypeConfiguration<InvoiceToCollectionProcess>
{
    public void Configure(EntityTypeBuilder<InvoiceToCollectionProcess> builder)
    {
        builder
            .ToTable(Tables.InvoiceToCollectionProcesses);

        builder
            .HasKey(field => field.CollectionId);

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(p => p.CollectionId);
    }
}

public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<InvoiceToCollectionProcessItem>
{
    public void Configure(EntityTypeBuilder<InvoiceToCollectionProcessItem> builder)
    {
        builder
            .ToTable(Tables.InvoiceToCollectionProcessItems);

        builder
            .HasKey(i => new { i.CollectionId, i.InvoiceId });
    }
}