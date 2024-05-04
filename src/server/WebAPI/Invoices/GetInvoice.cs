using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.ProformaToInvoiceProcesses;

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
        public Guid ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? Number { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? IssueAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
        public decimal ITF { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Invoices)
                .Select(Tables.Invoices.AllFields)
                .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
                .Join(Tables.Clients, Tables.Invoices.Field(nameof(Invoice.ClientId)), Tables.Clients.Field(nameof(Invoice.ClientId)))
                .Where(Tables.Invoices.Field(nameof(Invoice.InvoiceId)), query.InvoiceId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid invoiceId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { InvoiceId = invoiceId }));
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] Runner runner,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItems,
    [FromRoute] Guid invoiceId)
    {
        var result = await runner.Run(new Query() { InvoiceId = invoiceId });

        var listProformaToInvoiceProcessItemsQuery = new ListProformaToInvoiceProcessItems.Query() { InvoiceId = invoiceId, PageSize = 5 };

        var listProformaToInvoiceProcessItemsResult = await listProformaToInvoiceProcessItems.Run(listProformaToInvoiceProcessItemsQuery);

        return new RazorComponentResult<GetInvoicePage>(new
        {
            Result = result,
            ListProformaToInvoiceProcessItemsResult = listProformaToInvoiceProcessItemsResult,
            ListProformaToInvoiceProcessItemsQuery = listProformaToInvoiceProcessItemsQuery,
        });
    }
}
