using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Invoices;

public static class SearchInvoicesNotAddedToCollection
{
    public class Query
    {
        public Guid? ClientId { get; set; }
        public string? Currency { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
        public Guid ClientId { get; set; }
        public string? Number { get; set; }
        public string? Currency { get; set; }
        public decimal Total { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.VwNotAddedToCollectionInvoices)
                .Where(Tables.VwNotAddedToCollectionInvoices.Field(nameof(Invoice.Status)), InvoiceStatus.Issued.ToString())
                ;
                if (query.ClientId.HasValue)
                {
                    statement = statement.Where(Tables.VwNotAddedToCollectionInvoices.Field(nameof(Invoice.ClientId)), query.ClientId);
                }
                if (!string.IsNullOrEmpty(query.Currency))
                {
                    statement = statement.Where(Tables.VwNotAddedToCollectionInvoices.Field(nameof(Invoice.Currency)), query.Currency);
                }
                return statement;
            });
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);

        return new RazorComponentResult<SearchInvoicesNotAddedToCollectionPage>(new { Result = result });
    }
}
