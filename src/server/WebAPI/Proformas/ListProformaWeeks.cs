using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class ListProformaWeeks
{
    public class Query : ListQuery
    {
        public Guid? ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Penalty { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.ProformaWeeks)
                .Where(Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)), query.ProformaId), query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid proformaId,
    [AsParameters] Query query)
    {
        query.ProformaId = proformaId;
        return TypedResults.Ok(await runner.Run(query));
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);
        return new RazorComponentResult<ListProformaWeeksPage>(new { Result = result, Query = query });
    }
}
