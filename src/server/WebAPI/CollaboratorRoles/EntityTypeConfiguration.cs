using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.CollaboratorRoles;

public class EntityTypeConfiguration : IEntityTypeConfiguration<CollaboratorRole>
{
    public void Configure(EntityTypeBuilder<CollaboratorRole> builder)
    {
        builder
            .ToTable(Tables.CollaboratorRoles);

        builder
            .HasKey(cr => cr.CollaboratorRoleId);

        builder
            .Property(c => c.ProfitPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FeeAmount)
            .HasColumnType("decimal(19, 4)");
    }
}