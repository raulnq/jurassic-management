using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.JiraProfiles;

public class JiraProfile
{
    public Guid ClientId { get; private set; }
    public string TempoToken { get; private set; } = default!;
    private JiraProfile() { }

    public JiraProfile(Guid clientId, string tempoToken)
    {
        ClientId = clientId;
        TempoToken = tempoToken;
    }
}

public class JiraProfileProject
{
    public Guid ClientId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string JiraProjectId { get; private set; } = default!;
    private JiraProfileProject() { }
}

public class JiraProfileAccount
{
    public Guid ClientId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public Guid CollaboratorRoleId { get; set; }
    public string JiraAccountId { get; private set; } = default!;

    private JiraProfileAccount() { }
}

public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<JiraProfileProject>
{
    public void Configure(EntityTypeBuilder<JiraProfileProject> builder)
    {
        builder
            .ToTable(Tables.JiraProfileProjects);

        builder
            .HasKey(j => new { j.ClientId, j.ProjectId });

    }
}