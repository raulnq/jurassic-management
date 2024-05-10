using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.JiraProfiles;

public static class ListJiraProfileAccounts
{
    public class Query
    {
        public Guid ClientId { get; set; }
    }

    public class Result
    {
        public Guid ClientId { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public string JiraAccountId { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.JiraProfileAccounts)
                .Where(Tables.JiraProfileAccounts.Field(nameof(JiraProfileAccount.ClientId)), query.ClientId);
                return statement;
            });
        }
    }
}