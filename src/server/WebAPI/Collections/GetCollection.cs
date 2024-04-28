using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collections;

public static class GetCollection
{
    public class Query
    {
        public Guid CollectionId { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; private set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Collections)
                .Where(Tables.Collections.Field(nameof(Query.CollectionId)), query.CollectionId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid collectionId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { CollectionId = collectionId }));
    }
}
