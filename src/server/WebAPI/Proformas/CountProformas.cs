using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class CountProformas
{
    public class Query
    {
        public DateTime? End { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<int> Run(Query query)
        {
            return _queryRunner.Count((qf) =>
            {
                var statement = qf.Query(Tables.Proformas);

                if (query.End.HasValue)
                {
                    statement = statement.Where(Tables.Proformas.Field(nameof(Proforma.End)), query.End.Value);
                }

                return statement;
            });
        }
    }
}