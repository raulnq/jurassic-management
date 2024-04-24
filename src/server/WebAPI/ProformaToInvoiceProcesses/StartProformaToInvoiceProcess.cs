using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.ProformaToInvoiceProcesses;

public static class StartProformaToInvoiceProcess
{
    public class Command
    {
        public IEnumerable<Guid>? ProformaId { get; set; }
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
            var process = new ProformaToInvoiceProcess(NewId.Next().ToSequentialGuid(), command.Proformas!.Select(p => (p.ProformaId, p.Status!.ToEnum<ProformaStatus>())), command.CreatedAt);

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

        //TODO: Todas las proformas tienen que ser de la misma moneda

        var currency = proformas.Items.First().Currency;

        command.Proformas = proformas.Items;

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            await registerInvoiceHandler.Handle(new RegisterInvoice.Command() { InvoiceId = result.InvoiceId, CreatedAt = command.CreatedAt, SubTotal = proformas.Items.Sum(p => p.Total), Taxes = 0, Currency = currency.ToEnum<Currency>() });

            return result;
        });

        return TypedResults.Ok(result);
    }
}
