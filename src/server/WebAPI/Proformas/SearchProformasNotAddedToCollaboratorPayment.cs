using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class SearchProformasNotAddedToCollaboratorPayment
{
    public class Query
    {
        public Guid? CollaboratorId { get; set; }
        public string? Currency { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public Guid CollaboratorId { get; set; }
        public int Week { get; set; }
        public Guid ProjectId { get; set; }
        public string? Number { get; set; }
        public DateTime Start { get; set; }
        public string? Currency { get; set; }
        public DateTime End { get; set; }
        public decimal Hours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.VwNotAddedToCollaboratorPaymentProformas)
                .Join(Tables.Collaborators, Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)), Tables.VwNotAddedToCollaboratorPaymentProformas.Field(nameof(ProformaWeekWorkItem.CollaboratorId)))
                .Where(Tables.VwNotAddedToCollaboratorPaymentProformas.Field(nameof(Proforma.Status)), ProformaStatus.Issued.ToString())
                ;
                if (query.CollaboratorId.HasValue)
                {
                    statement = statement.Where(Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)), query.CollaboratorId);
                }
                if (!string.IsNullOrEmpty(query.Currency))
                {
                    statement = statement.Where(Tables.VwNotAddedToCollaboratorPaymentProformas.Field(nameof(Proforma.Currency)), query.Currency);
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

        return new RazorComponentResult<SearchProformasNotAddedToCollaboratorPaymentPage>(new { Result = result });
    }
}
