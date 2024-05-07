using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Balance;

public static class GetBalance
{
    public class Query
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Result
    {
        public decimal Total { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>(qf =>
            {
                var statement = qf
                .Query(Tables.VwBalanceRecords)
                .SelectRaw("SUM(Total*Sign-ITF) as Total")
                .Where(Tables.VwBalanceRecords.Field(Tables.VwBalanceRecords.Field("Currency")), query.Currency);

                if (query.End.HasValue && query.Start.HasValue)
                {
                    statement = statement.WhereBetween(Tables.VwBalanceRecords.Field("IssuedAt"), query.Start, query.End);
                }
                else
                {
                    if (query.Start.HasValue)
                    {
                        statement = statement.Where(Tables.VwBalanceRecords.Field("IssuedAt"), ">=", query.Start);
                    }

                    if (query.End.HasValue)
                    {
                        statement = statement.Where(Tables.VwBalanceRecords.Field("IssuedAt"), "<=", query.End);
                    }
                }

                return statement;
            });
        }
    }
}
