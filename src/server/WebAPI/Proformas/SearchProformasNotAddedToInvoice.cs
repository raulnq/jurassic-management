using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Projects;

namespace WebAPI.Proformas;

public static class SearchProformasNotAddedToInvoice
{
    public class Query
    {
        public Guid? ClientId { get; set; }
        public string? Currency { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public Guid ProjectId { get; set; }
        public string? Number { get; set; }
        public DateTime Start { get; set; }
        public string? Currency { get; set; }
        public DateTime End { get; set; }
        public decimal Total { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.VwNotAddedToInvoiceProformas)
                .Join(Tables.Projects, Tables.Projects.Field(nameof(Proforma.ProjectId)), Tables.VwNotAddedToInvoiceProformas.Field(nameof(Proforma.ProjectId)))
                .Where(Tables.VwNotAddedToInvoiceProformas.Field(nameof(Proforma.Status)), ProformaStatus.Issued.ToString())
                ;
                if (query.ClientId.HasValue)
                {
                    statement = statement.Where(Tables.Projects.Field(nameof(Project.ClientId)), query.ClientId);
                }
                if (!string.IsNullOrEmpty(query.Currency))
                {
                    statement = statement.Where(Tables.VwNotAddedToInvoiceProformas.Field(nameof(Proforma.Currency)), query.Currency);
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

        return new RazorComponentResult<SearchProformasNotAddedToInvoicePage>(new { Result = result });
    }
}
