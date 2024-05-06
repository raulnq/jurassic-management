using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToCollaboratorPaymentProcesses;

namespace WebAPI.CollaboratorPayments;

public static class CancelCollaboratorPayment
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollaboratorPaymentId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CanceledAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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
            var collaboratorPayment = await _context.Get<CollaboratorPayment>(command.CollaboratorPaymentId);

            if (collaboratorPayment == null)
            {
                throw new NotFoundException<CollaboratorPayment>();
            }

            collaboratorPayment.Cancel(command.CanceledAt);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collaboratorPaymentId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.CollaboratorPaymentId = collaboratorPaymentId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollaboratorPayment.Runner getCollaboratorPaymentRunner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    Guid collaboratorPaymentId,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var command = new Command()
        {
            CollaboratorPaymentId = collaboratorPaymentId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, handler, collaboratorPaymentId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("collaborator payment", "canceled", collaboratorPaymentId);

        return await GetCollaboratorPayment.HandlePage(getCollaboratorPaymentRunner, listProformaToCollaboratorPaymentProcessItemsRunner, collaboratorPaymentId);
    }
}
