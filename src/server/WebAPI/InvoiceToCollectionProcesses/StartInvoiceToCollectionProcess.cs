using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Collections;
using WebAPI.Infrastructure.EntityFramework;
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
            var process = new InvoiceToCollectionProcess(NewId.Next().ToSequentialGuid(), command.Invoices!.Select(p => (p.InvoiceId, p.Status!.ToEnum<InvoiceStatus>())), command.CreatedAt);

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

        //TODO: Todas las facturas tienen que ser de la misma moneda

        var currency = invoices.Items.First().Currency;

        command.Invoices = invoices.Items;

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            await registerCollectionHandler.Handle(new RegisterCollection.Command() { CollectionId = result.CollectionId, CreatedAt = command.CreatedAt, Total = invoices.Items.Sum(p => p.Total), Currency = currency.ToEnum<Currency>() });

            return result;
        });

        return TypedResults.Ok(result);
    }
}
