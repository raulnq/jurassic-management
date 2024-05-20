using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
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

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid invoiceId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.InvoiceId = invoiceId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var invoice = await dbContext.Get<Invoice>(command.InvoiceId);

            if (invoice == null)
            {
                throw new NotFoundException<Invoice>();
            }

            invoice.Cancel(command.CanceledAt);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
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

        await Handle(behavior, dbContext, invoiceId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("invoice", "canceled", command.InvoiceId);

        return await GetInvoice.HandlePage(runner, listProformaToInvoiceProcessItems, command.InvoiceId);
    }
}
