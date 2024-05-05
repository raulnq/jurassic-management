using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToInvoiceProcesses;

namespace WebAPI.Invoices;

public static class CancelInvoice
{
    public class Command
    {
        [JsonIgnore]
        public Guid InvoiceId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CanceledAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.InvoiceId).NotEmpty();
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
            var invoice = await _context.Get<Invoice>(command.InvoiceId);

            if (invoice == null)
            {
                throw new NotFoundException<Invoice>();
            }

            invoice.Cancel(command.CanceledAt);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid invoiceId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.InvoiceId = invoiceId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetInvoice.Runner getInvoiceRunner,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItems,
    [FromServices] IClock clock,
    HttpContext context,
    Guid invoiceId)
    {
        var command = new Command()
        {
            InvoiceId = invoiceId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, handler, invoiceId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage($"The invoice {command.InvoiceId} was canceled successfully");

        return await GetInvoice.HandlePage(getInvoiceRunner, listProformaToInvoiceProcessItems, command.InvoiceId);
    }
}
