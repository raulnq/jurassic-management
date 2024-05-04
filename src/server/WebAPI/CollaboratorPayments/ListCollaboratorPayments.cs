using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace WebAPI.CollaboratorPayments;

public static class ListCollaboratorPayments
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Withholding { get; set; }
        public string? Status { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Number { get; set; }
        public string? CollaboratorName { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.CollaboratorPayments)
                .Select(Tables.CollaboratorPayments.AllFields)
                .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
                .Join(Tables.Collaborators, Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)));

                if (!string.IsNullOrEmpty(query.Status))
                {
                    statement = statement.Where(Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.Status)), query.Status);
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
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);
        return new RazorComponentResult<ListCollaboratorPaymentsPage>(new { Result = result, Query = query });
    }
}
