using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaToCollaboratorPaymentProcess>
{
    public void Configure(EntityTypeBuilder<ProformaToCollaboratorPaymentProcess> builder)
    {
        builder
            .ToTable(Tables.ProformaToCollaboratorPaymentProcesses);

        builder
            .HasKey(field => field.CollaboratorPaymentId);

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(p => p.CollaboratorPaymentId);
    }
}

public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaToCollaboratorPaymentProcessItem>
{
    public void Configure(EntityTypeBuilder<ProformaToCollaboratorPaymentProcessItem> builder)
    {
        builder
            .ToTable(Tables.ProformaToCollaboratorPaymentProcessItems);

        builder
            .HasKey(i => new { i.CollaboratorPaymentId, i.ProformaId, i.Week, i.CollaboratorId });
    }
}