using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.Collaborators;

public class EntityTypeConfiguration : IEntityTypeConfiguration<Collaborator>
{
    public void Configure(EntityTypeBuilder<Collaborator> builder)
    {
        builder
            .ToTable(Tables.Collaborators);

        builder
            .HasKey(c => c.CollaboratorId);

        builder
            .Property(c => c.WithholdingPercentage)
            .HasColumnType("decimal(19, 4)");
    }
}