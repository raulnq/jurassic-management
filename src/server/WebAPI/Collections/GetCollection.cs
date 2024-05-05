using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.InvoiceToCollectionProcesses;

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
        public Guid ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
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
                .Select(Tables.Collections.AllFields)
                .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
                .Join(Tables.Clients, Tables.Collections.Field(nameof(Collection.ClientId)), Tables.Clients.Field(nameof(Client.ClientId)))
                .Where(Tables.Collections.Field(nameof(Query.CollectionId)), query.CollectionId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid collectionId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { CollectionId = collectionId }));
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] Runner runner,
    [FromServices] ListInvoiceToCollectionProcessItems.Runner listInvoiceToCollectionProcessItemsRunner,
    [FromRoute] Guid collectionId)
    {
        var result = await runner.Run(new Query() { CollectionId = collectionId });

        var listInvoiceToCollectionProcessItemsQuery = new ListInvoiceToCollectionProcessItems.Query() { CollectionId = collectionId, PageSize = 5 };

        var listInvoiceToCollectionProcessItemsResult = await listInvoiceToCollectionProcessItemsRunner.Run(listInvoiceToCollectionProcessItemsQuery);

        return new RazorComponentResult<GetCollectionPage>(new
        {
            Result = result,
            ListInvoiceToCollectionProcessItemsResult = listInvoiceToCollectionProcessItemsResult,
            ListInvoiceToCollectionProcessItemsQuery = listInvoiceToCollectionProcessItemsQuery,
        });
    }
}
