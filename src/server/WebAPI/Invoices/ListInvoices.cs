using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Invoices;

public static class ListInvoices
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }
        public Guid? ClientId { get; set; }
        public IEnumerable<Guid>? InvoiceId { get; set; }
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
        public string Currency { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.Invoices)
                .Select(Tables.Invoices.AllFields)
                .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
                .Join(Tables.Clients, Tables.Invoices.Field(nameof(Invoice.ClientId)), Tables.Clients.Field(nameof(Client.ClientId)));

                if (!string.IsNullOrEmpty(query.Status))
                {
                    statement = statement.Where(Tables.Invoices.Field(nameof(Invoice.Status)), query.Status);
                }
                if (query.ClientId.HasValue && query.ClientId != Guid.Empty)
                {
                    statement = statement.Where(Tables.Invoices.Field(nameof(Invoice.ClientId)), query.ClientId);
                }
                if (query.InvoiceId != null && query.InvoiceId.Any())
                {
                    statement = statement.WhereIn(Tables.Invoices.Field(nameof(Invoice.InvoiceId)), query.InvoiceId);
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

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SearchClients.Runner searchClientsRunner,
    [FromServices] Runner runner)
    {
        var clients = await searchClientsRunner.Run(new SearchClients.Query() { });

        var result = await runner.Run(query);

        return new RazorComponentResult<ListInvoicesPage>(new { Result = result, Query = query, Clients = clients });
    }
}
