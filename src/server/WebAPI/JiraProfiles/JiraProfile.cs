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