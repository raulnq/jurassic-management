using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;
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
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItemsRunner,
    [FromServices] MarkProformaAsIssued.Handler markProformaAsIssuedHandler,
    [FromRoute] Guid invoiceId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.InvoiceId = invoiceId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            await handler.Handle(command);

            var result = await listProformaToInvoiceProcessItemsRunner.Run(new ListProformaToInvoiceProcessItems.Query() { InvoiceId = invoiceId, PageSize = 100 });

            foreach (var item in result.Items)
            {
                await markProformaAsIssuedHandler.Handle(new MarkProformaAsIssued.Command() { ProformaId = item.ProformaId });
            }
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetInvoice.Runner getInvoiceRunner,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItems,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItemsRunner,
    [FromServices] MarkProformaAsIssued.Handler markProformaAsIssuedHandler,
    [FromServices] IClock clock,
    HttpContext context,
    Guid invoiceId)
    {
        var command = new Command()
        {
            InvoiceId = invoiceId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, handler, listProformaToInvoiceProcessItemsRunner, markProformaAsIssuedHandler, invoiceId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage($"The invoice {command.InvoiceId} was cancel successfully");

        return await GetInvoice.HandlePage(getInvoiceRunner, listProformaToInvoiceProcessItems, command.InvoiceId);
    }
}
