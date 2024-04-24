using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;

namespace WebAPI.Proformas;

public class ProformaEntityTypeConfiguration : IEntityTypeConfiguration<Proforma>
{
    public void Configure(EntityTypeBuilder<Proforma> builder)
    {
        builder
            .ToTable(Tables.Proformas);

        builder
            .HasKey(p => p.ProformaId);

        builder
            .Property(c => c.Total)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Commission)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Discount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.MinimumHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.PenaltyMinimumHours)
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

        builder
            .Property(c => c.TaxesExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.AdministrativeExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.BankingExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<ProformaStatus>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Weeks)
            .WithOne()
            .HasForeignKey(pw => pw.ProformaId);
    }
}

public class WeekEntityTypeConfiguration : IEntityTypeConfiguration<ProformaWeek>
{
    public void Configure(EntityTypeBuilder<ProformaWeek> builder)
    {
        builder
            .ToTable(Tables.ProformaWeeks);

        builder
            .HasKey(field => new { field.ProformaId, field.Week });

        builder
            .Property(c => c.Penalty)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");


        builder
            .HasMany(p => p.WorkItems)
            .WithOne()
            .HasForeignKey(wi => new { wi.ProformaId, wi.Week });
    }
}

public class WorkItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaWeekWorkItem>
{
    public void Configure(EntityTypeBuilder<ProformaWeekWorkItem> builder)
    {
        builder
            .ToTable(Tables.ProformaWeekWorkItems);

        builder
            .HasKey(field => new { field.ProformaId, field.Week, field.CollaboratorId });

        builder
            .Property(c => c.Hours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FreeHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FeeAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ProfitAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ProfitPercentage)
            .HasColumnType("decimal(19, 4)");

    }
}