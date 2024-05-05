using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Clients;
using WebAPI.Collections;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.InvoiceToCollectionProcesses;

public static class StartInvoiceToCollectionProcess
{
    public class Command
    {
        public IEnumerable<Guid>? InvoiceId { get; set; }
        [JsonIgnore]
        public IEnumerable<ListInvoices.Result>? Invoices { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        public Guid ClientId { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
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

        public Task<Result> Handle(Command command)
        {
            var process = new InvoiceToCollectionProcess(NewId.Next().ToSequentialGuid(), command.ClientId, command.Currency, command.Invoices!.Select(p => (p.InvoiceId, p.Status!.ToEnum<InvoiceStatus>())), command.CreatedAt);

            _context.Set<InvoiceToCollectionProcess>().Add(process);

            return Task.FromResult(new Result()
            {
                CollectionId = process.CollectionId
            });
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] RegisterCollection.Handler registerCollectionHandler,
    [FromServices] ListInvoices.Runner listInvoiceHandler,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        command.CreatedAt = clock.Now;

        var invoices = await listInvoiceHandler.Run(new ListInvoices.Query() { InvoiceId = command.InvoiceId, PageSize = 50 });

        command.Invoices = invoices.Items;

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            await registerCollectionHandler.Handle(new RegisterCollection.Command()
            {
                CollectionId = result.CollectionId,
                ClientId = command.ClientId,
                CreatedAt = command.CreatedAt,
                Total = invoices.Items.Sum(p => p.Total),
                Currency = command.Currency
            });

            return result;
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SearchClients.Runner runner)
    {
        var result = await runner.Run(new SearchClients.Query() { });

        return new RazorComponentResult<StartInvoiceToCollectionProcessPage>(new
        {
            Clients = result
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] RegisterCollection.Handler registerCollectionHandler,
    [FromServices] ListInvoices.Runner listInvoicesRunner,
    [FromServices] ListCollections.Runner listCollectionsRunner,
    [FromServices] SearchClients.Runner searchClientsRunner,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var register = await Handle(behavior, handler, registerCollectionHandler, listInvoicesRunner, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collection", register.Value!.CollectionId);

        return await ListCollections.HandlePage(new ListCollections.Query() { }, searchClientsRunner, listCollectionsRunner);
    }
}
