using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToInvoiceProcesses;


namespace WebAPI.Invoices;

public static class IssueInvoice
{
    public class Command
    {
        [JsonIgnore]
        public Guid InvoiceId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Number { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.InvoiceId).NotEmpty();
            RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid invoiceId,
    [FromBody] Command command)
    {
        command.InvoiceId = invoiceId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var invoice = await dbContext.Get<Invoice>(command.InvoiceId);

            invoice.Issue(command.IssuedAt, command.Number);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid invoiceId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<IssueInvoicePage>(new
        {
            InvoiceId = invoiceId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext bdContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItemsRunner,
    [FromBody] Command command,
    Guid invoiceId,
    HttpContext context)
    {
        await Handle(behavior, bdContext, invoiceId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("invoice", "issued", invoiceId);

        return await GetInvoice.HandlePage(runner, listProformaToInvoiceProcessItemsRunner, invoiceId);
    }
}