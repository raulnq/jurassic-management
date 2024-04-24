using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.CollaboratorPayments;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class StartProformaToCollaboratorPaymentProcess
{
    public class Command
    {
        public IEnumerable<Guid>? ProformaId { get; set; }
        [JsonIgnore]
        public IEnumerable<ListProformaWeekWorkItems.Result>? ProformaWeekWorkItems { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
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
            var process = new ProformaToCollaboratorPaymentProcess(NewId.Next().ToSequentialGuid(), command.ProformaWeekWorkItems!.Select(p => (p.ProformaId, p.Week, p.CollaboratorId, p.Status!.ToEnum<ProformaStatus>())), command.CreatedAt);

            _context.Set<ProformaToCollaboratorPaymentProcess>().Add(process);

            return Task.FromResult(new Result()
            {
                CollaboratorPaymentId = process.CollaboratorPaymentId
            });
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] RegisterCollaboratorPayment.Handler registerCollaboratorPaymentHandler,
    [FromServices] ListProformaWeekWorkItems.Runner listProformaWeekWorkItemsHandler,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        command.CreatedAt = clock.Now;

        var proformaWeekWorkItems = await listProformaWeekWorkItemsHandler.Run(new ListProformaWeekWorkItems.Query() { ListOfProformaId = command.ProformaId, PageSize = 100 });

        command.ProformaWeekWorkItems = proformaWeekWorkItems.Items;

        //TODO: Todas los items de la proforma tienen que ser de la misma moneda

        var currency = proformaWeekWorkItems.Items.First().Currency;

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            foreach (var collaborator in proformaWeekWorkItems.Items.GroupBy(i => new { i.ProformaId, i.Week, i.CollaboratorId }))
            {
                await registerCollaboratorPaymentHandler.Handle(new RegisterCollaboratorPayment.Command()
                {
                    CollaboratorPaymentId = result.CollaboratorPaymentId,
                    CollaboratorId = collaborator.Key.CollaboratorId,
                    Week = collaborator.Key.Week,
                    ProformaId = collaborator.Key.ProformaId,
                    CreatedAt = command.CreatedAt,
                    GrossSalary = collaborator.Sum(i => i.SubTotal - i.ProfitAmount),
                    WithholdingPercentage = collaborator.Average(i => i.WithholdingPercentage),
                    Currency = currency.ToEnum<Currency>()
                });
            }

            return result;
        });

        return TypedResults.Ok(result);
    }
}
