using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace WebAPI.ProformaDocuments;

public static class GetProformaDocument
{
    public class Query
    {
        public Guid ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public string Url { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.GetOrDefault<Result>((qf) => qf
                .Query(Tables.ProformaDocuments)
                .Where(Tables.ProformaDocuments.Field(nameof(Proforma.ProformaId)), query.ProformaId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid proformaId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { ProformaId = proformaId }));
    }
}
