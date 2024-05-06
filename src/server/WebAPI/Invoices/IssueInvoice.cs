using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToInvoiceProcesses;


namespace WebAPI.Invoices;

public static class IssueInvoice
{
    public class Command
    {
        [JsonIgnore]
        public Guid InvoiceId { get; set; }
        public DateTime IssueAt { get; set; }
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

            invoice.Issue(command.IssueAt, command.Number);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid invoiceId,
    [FromBody] Command command)
    {
        command.InvoiceId = invoiceId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

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
    [FromServices] Handler handler,
    [FromServices] GetInvoice.Runner getInvoiceRunner,
    [FromServices] ListProformaToInvoiceProcessItems.Runner listProformaToInvoiceProcessItemsRunner,
    [FromBody] Command command,
    Guid invoiceId,
    HttpContext context)
    {
        await Handle(behavior, handler, invoiceId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("invoice", "issued", invoiceId);

        return await GetInvoice.HandlePage(getInvoiceRunner, listProformaToInvoiceProcessItemsRunner, invoiceId);
    }
}