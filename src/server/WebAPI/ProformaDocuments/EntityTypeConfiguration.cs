using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.ProformaDocuments;

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaDocument>
{
    public void Configure(EntityTypeBuilder<ProformaDocument> builder)
    {
        builder
            .ToTable(Tables.ProformaDocuments);

        builder
            .HasKey(p => p.ProformaId);
    }
}