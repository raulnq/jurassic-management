using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToCollaboratorPaymentProcesses;


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

            payment.Confirm(command.ConfirmedAt, command.Number);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collaboratorPaymentId,
    [FromBody] Command command)
    {
        command.CollaboratorPaymentId = collaboratorPaymentId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

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
    [FromServices] Handler handler,
    [FromServices] GetCollaboratorPayment.Runner getCollaboratorPaymentRunner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    [FromBody] Command command,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, handler, collaboratorPaymentId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collaborator payment", "confirmed", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(getCollaboratorPaymentRunner, listProformaToCollaboratorPaymentProcessItemsRunner, collaboratorPaymentId);
    }
}