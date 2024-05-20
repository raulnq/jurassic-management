using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.ProformaToInvoiceProcesses;

public static class StartProformaToInvoiceProcess
{
    public class Command
    {
        public IEnumerable<Guid>? ProformaId { get; set; }
        public Guid ClientId { get; set; }
        public Currency Currency { get; set; }
        [JsonIgnore]
        public IEnumerable<ListProformas.Result>? Proformas { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result> Handle(Command command)
        {
            var process = new ProformaToInvoiceProcess(NewId.Next().ToSequentialGuid(), command.ClientId, command.Currency, command.Proformas!.Select(p => (p.ProformaId, p.Status!.ToEnum<ProformaStatus>())), command.CreatedAt);

            _context.Set<ProformaToInvoiceProcess>().Add(process);

            return Task.FromResult(new Result()
            {
                InvoiceId = process.InvoiceId
            });
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] RegisterInvoice.Handler registerInvoiceHandler,
    [FromServices] ListProformas.Runner listProformasHandler,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        command.CreatedAt = clock.Now;

        var proformas = await listProformasHandler.Run(new ListProformas.Query() { ProformaId = command.ProformaId, PageSize = 50 });

        command.Proformas = proformas.Items;

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            await registerInvoiceHandler.Handle(new RegisterInvoice.Command()
            {
                InvoiceId = result.InvoiceId,
                CreatedAt = command.CreatedAt,
                SubTotal = proformas.Items.Sum(p => p.Total),
                Taxes = 0,
                Currency = command.Currency,
                ClientId = command.ClientId,
            });

            return result;
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext)
    {
        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<StartProformaToInvoiceProcessPage>(new
        {
            Clients = clients
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListProformas.Runner listProformasRunner,
    [FromServices] RegisterInvoice.Handler registerInvoiceHandler,
    [FromServices] ListInvoices.Runner listInvoicesRunner,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var register = await Handle(behavior, handler, registerInvoiceHandler, listProformasRunner, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"invoice", register.Value!.InvoiceId);

        return await ListInvoices.HandlePage(new ListInvoices.Query() { }, dbContext, listInvoicesRunner);
    }
}
