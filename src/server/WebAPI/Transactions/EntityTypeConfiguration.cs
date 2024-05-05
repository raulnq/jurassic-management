using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.Transactions;

public class EntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .ToTable(Tables.Transactions);

        builder
            .HasKey(cr => cr.TransactionId);

        builder
            .Property(c => c.Total)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Taxes)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Type)
            .HasConversion(s => s.ToString(), value => value.ToEnum<TransactionType>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}