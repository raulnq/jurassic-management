using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collaborators;

public static class SearchCollaborators
{
    public class Query
    {
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
        public string? Name { get; set; }
        public decimal WithholdingPercentage { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.Collaborators);
                return statement;
            });
        }
    }
}
