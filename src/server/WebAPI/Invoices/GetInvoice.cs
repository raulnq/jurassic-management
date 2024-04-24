using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Invoices;

public static class GetInvoice
{
    public class Query
    {
        public Guid InvoiceId { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? IssueAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Invoices)
                .Where(Tables.Invoices.Field(nameof(Query.InvoiceId)), query.InvoiceId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid invoiceId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { InvoiceId = invoiceId }));
    }
}
