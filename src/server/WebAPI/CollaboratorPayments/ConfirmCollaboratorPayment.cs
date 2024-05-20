using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.CollaboratorPayments;

public static class ConfirmCollaboratorPayment
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollaboratorPaymentId { get; set; }
        public string Number { get; set; } = default!;
        public DateTime ConfirmedAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
            RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorPaymentId,
    [FromBody] Command command)
    {
        command.CollaboratorPaymentId = collaboratorPaymentId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var payment = await dbContext.Get<CollaboratorPayment>(command.CollaboratorPaymentId);

            payment.Confirm(command.ConfirmedAt, command.Number);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collaboratorPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<ConfirmCollaboratorPaymentPage>(new
        {
            CollaboratorPaymentId = collaboratorPaymentId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, collaboratorPaymentId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collaborator payment", "confirmed", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}