using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.Collaborators;

public class Collaborator
{
    public Guid CollaboratorId { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal WithholdingPercentage { get; private set; }

    private Collaborator() { }

    public Collaborator(Guid collaboratorId, string name, decimal withholdingPercentage)
    {
        CollaboratorId = collaboratorId;
        Name = name;
        WithholdingPercentage = withholdingPercentage;
    }

    public void Edit(string name, decimal withholdingPercentage)
    {
        Name = name;
        WithholdingPercentage = withholdingPercentage;
    }
}

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