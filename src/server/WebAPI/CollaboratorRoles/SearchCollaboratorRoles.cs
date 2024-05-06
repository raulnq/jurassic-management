using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorRoles;

public static class SearchCollaboratorRoles
{
    public class Query
    {
    }

    public class Result
    {
        public Guid CollaboratorRoleId { get; set; }
        public string Name { get; set; } = default!;
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) => qf.Query(Tables.CollaboratorRoles));
        }
    }
}
