using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collections;

public static class ListCollections
{
    public class Query : ListQuery
    {
        public CollectionStatus? Status { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; private set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.Collections);

                if (query.Status.HasValue)
                {
                    statement = statement.Where(Tables.Collections.Field(nameof(Query.Status)), query.Status.ToString());
                }
                return statement;
            }, query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] Runner runner,
    [AsParameters] Query query)
    {
        return TypedResults.Ok(await runner.Run(query));
    }
}
