using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;
using WebAPI.Projects;

namespace WebAPI.ProformaToInvoiceProcesses;

public static class ListProformaToInvoiceProcessItems
{
    public class Query : ListQuery
    {
        public Guid? InvoiceId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public Guid InvoiceId { get; set; }
        public string? ProformaProjectName { get; set; }
        public DateTime ProformaStart { get; set; }
        public DateTime ProformaEnd { get; set; }
        public string? ProformaNumber { get; set; }
        public string? ProformaCurrency { get; set; }
        public decimal ProformTotal { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.ProformaToInvoiceProcessItems)
                .Select(Tables.ProformaToInvoiceProcessItems.AllFields)
                .Select(Tables.Projects.Field(nameof(Project.Name), nameof(Result.ProformaProjectName)))
                .Select(
                    Tables.Proformas.Field(nameof(Proforma.Start), nameof(Result.ProformaStart)),
                    Tables.Proformas.Field(nameof(Proforma.End), nameof(Result.ProformaEnd)),
                    Tables.Proformas.Field(nameof(Proforma.Number), nameof(Result.ProformaNumber)),
                    Tables.Proformas.Field(nameof(Proforma.Currency), nameof(Result.ProformaCurrency)),
                    Tables.Proformas.Field(nameof(Proforma.Total), nameof(Result.ProformTotal))
                    )
                .Join(Tables.Proformas, Tables.ProformaToInvoiceProcessItems.Field(nameof(ProformaToInvoiceProcessItem.ProformaId)), Tables.Proformas.Field(nameof(Proforma.ProformaId)))
                .Join(Tables.Projects, Tables.Proformas.Field(nameof(Proforma.ProjectId)), Tables.Projects.Field(nameof(Project.ProjectId)))
                .Where(Tables.ProformaToInvoiceProcessItems.Field(nameof(ProformaToInvoiceProcessItem.InvoiceId)), query.InvoiceId), query);
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);
        return new RazorComponentResult<ListProformaToInvoiceProcessItemsPage>(new { Result = result, Query = query });
    }
}
