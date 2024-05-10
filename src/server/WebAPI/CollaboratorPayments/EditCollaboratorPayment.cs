using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;
using WebAPI.ProformaToCollaboratorPaymentProcesses;

namespace WebAPI.CollaboratorPayments;

public static class EditCollaboratorPayment
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollaboratorPaymentId { get; set; }
        public decimal GrossSalary { get; set; }
        public Currency Currency { get; set; }
        [JsonIgnore]
        public decimal WithholdingPercentage { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.GrossSalary).GreaterThan(0);
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command command)
        {
            var payment = await _context.Get<CollaboratorPayment>(command.CollaboratorPaymentId);

            payment.Edit(command.GrossSalary, command.Currency, command.WithholdingPercentage);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collaboratorPaymentId,
    [FromServices] GetCollaboratorPayment.Runner runner,
    [FromServices] GetCollaborator.Runner getCollaboratorRunner,
    [FromBody] Command command)
    {
        command.CollaboratorPaymentId = collaboratorPaymentId;

        var collaboratorPaymentResult = await runner.Run(new GetCollaboratorPayment.Query() { CollaboratorPaymentId = collaboratorPaymentId });

        var collaboratorResult = await getCollaboratorRunner.Run(new GetCollaborator.Query() { CollaboratorId = collaboratorPaymentResult.CollaboratorId });

        command.WithholdingPercentage = collaboratorResult.WithholdingPercentage;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollaboratorPayment.Runner getCollaboratorPaymentRunner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    [FromBody] Command command,
    [FromServices] GetCollaboratorPayment.Runner runner,
    [FromServices] GetCollaborator.Runner getCollaboratorRunner,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, handler, collaboratorPaymentId, runner, getCollaboratorRunner, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("collaborator payment", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(getCollaboratorPaymentRunner, listProformaToCollaboratorPaymentProcessItemsRunner, collaboratorPaymentId);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] GetCollaboratorPayment.Runner runner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    [FromRoute] Guid collaboratorPaymentId)
    {
        var result = await runner.Run(new GetCollaboratorPayment.Query() { CollaboratorPaymentId = collaboratorPaymentId });

        var listProformaToCollaboratorPaymentProcessItemsQuery = new ListProformaToCollaboratorPaymentProcessItems.Query() { CollaboratorPaymentId = collaboratorPaymentId, PageSize = 5 };

        var listProformaToCollaboratorPaymentProcessItemsResult = await listProformaToCollaboratorPaymentProcessItemsRunner.Run(listProformaToCollaboratorPaymentProcessItemsQuery);

        return new RazorComponentResult<EditCollaboratorPaymentPage>(new
        {
            Result = result,
            ListProformaToCollaboratorPaymentProcessItemsResult = listProformaToCollaboratorPaymentProcessItemsResult,
            ListProformaToCollaboratorPaymentProcessItemsQuery = listProformaToCollaboratorPaymentProcessItemsQuery,
        });
    }
}
