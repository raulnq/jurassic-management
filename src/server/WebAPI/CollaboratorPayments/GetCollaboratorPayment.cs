﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.ProformaToCollaboratorPaymentProcesses;
using WebAPI.ProformaToInvoiceProcesses;

namespace WebAPI.CollaboratorPayments;

public static class GetCollaboratorPayment
{
    public class Query
    {
        public Guid CollaboratorPaymentId { get; set; }
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

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.CollaboratorPayments)
                .Select(Tables.CollaboratorPayments.AllFields)
                .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
                .Join(Tables.Collaborators, Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)))
                .Where(Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorPaymentId)), query.CollaboratorPaymentId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid collaboratorPaymentId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { CollaboratorPaymentId = collaboratorPaymentId }));
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] Runner runner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    [FromRoute] Guid collaboratorPaymentId)
    {
        var result = await runner.Run(new Query() { CollaboratorPaymentId = collaboratorPaymentId });

        var listProformaToCollaboratorPaymentProcessItemsQuery = new ListProformaToCollaboratorPaymentProcessItems.Query() { CollaboratorPaymentId = collaboratorPaymentId, PageSize = 5 };

        var listProformaToCollaboratorPaymentProcessItemsResult = await listProformaToCollaboratorPaymentProcessItemsRunner.Run(listProformaToCollaboratorPaymentProcessItemsQuery);

        return new RazorComponentResult<GetCollaboratorPaymentPage>(new
        {
            Result = result,
            ListProformaToCollaboratorPaymentProcessItemsResult = listProformaToCollaboratorPaymentProcessItemsResult,
            ListProformaToCollaboratorPaymentProcessItemsQuery = listProformaToCollaboratorPaymentProcessItemsQuery,
        });
    }
}
