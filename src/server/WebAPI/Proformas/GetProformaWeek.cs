using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class GetProformaWeek
{
    public class Query
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
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

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.ProformaWeeks)
                .Where(Tables.ProformaWeeks.Field(nameof(Query.Week)), query.Week)
                .Where(Tables.ProformaWeeks.Field(nameof(Query.ProformaId)), query.ProformaId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid proformaId,
    [FromRoute] int week)
    {
        return TypedResults.Ok(await runner.Run(new Query() { Week = week, ProformaId = proformaId }));
    }
}
