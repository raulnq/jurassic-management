using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.Clients;

public class EntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder
            .ToTable(Tables.Clients);

        builder
            .HasKey(c => c.ClientId);

        builder
            .Property(c => c.PenaltyMinimumHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.PenaltyAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.TaxesExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.AdministrativeExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.BankingExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.MinimumBankingExpenses)
            .HasColumnType("decimal(19, 4)");
    }
}