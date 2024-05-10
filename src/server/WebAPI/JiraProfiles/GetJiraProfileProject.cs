using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.JiraProfiles;

public static class GetJiraProfileProject
{
    public class Query
    {
        public Guid ProjectId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string JiraProjectId { get; set; } = default!;
        public string TempoToken { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.GetOrDefault<Result>((qf) =>
            {
                var statement = qf.Query(Tables.JiraProfileProjects)
                .Select(Tables.JiraProfileProjects.AllFields)
                .Select(Tables.JiraProfiles.Field(nameof(JiraProfile.TempoToken)))
                .Join(Tables.JiraProfiles, Tables.JiraProfileProjects.Field(nameof(JiraProfileProject.ClientId)), Tables.JiraProfiles.Field(nameof(JiraProfile.ClientId)))
                .Where(Tables.JiraProfileProjects.Field(nameof(JiraProfileProject.ProjectId)), query.ProjectId);
                return statement;
            });
        }
    }
}
