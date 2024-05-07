using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace WebAPI.Balance;

public static class ListBalance
{
    public class Query : ListQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Result
    {
        public Guid RecordId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Type { get; set; } = default!;
        public string? Description { get; set; }
        public string Currency { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public decimal Balance { get; set; }
        public string? Number { get; set; }
        public string Source { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public int Sign { get; set; }
        public decimal Amount
        {
            get
            {
                return Total * Sign - ITF;
            }
        }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>(qf =>
            {
                var statement = qf
                .Query(Tables.VwBalanceRecords)
                .OrderBy(Tables.VwBalanceRecords.Field("IssuedAt"))
                .Where(Tables.VwBalanceRecords.Field("Currency"), query.Currency);

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

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] GetBalance.Runner getBalanceRunner,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);

        var startBalance = 0m;

        if (query.Start.HasValue)
        {
            startBalance = (await getBalanceRunner.Run(new GetBalance.Query() { Currency = query.Currency, End = query.Start.Value.AddDays(-1) })).Total;
        }

        var endBalance = startBalance;

        foreach (var item in result)
        {
            item.Balance = item.Amount + endBalance;
            endBalance = item.Amount + endBalance;
        }

        return new RazorComponentResult<ListBalancePage>(new { Result = result, Query = query, StartBalance = startBalance, EndBalance = endBalance });
    }
}
