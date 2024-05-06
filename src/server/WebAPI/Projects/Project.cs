namespace WebAPI.Projects;

public class Project
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    private Project() { }

    public Project(Guid projectId, Guid clientId, string name)
    {
        ProjectId = projectId;
        Name = name;
        ClientId = clientId;
    }

    public void Edit(string name)
    {
        Name = name;
    }
}
