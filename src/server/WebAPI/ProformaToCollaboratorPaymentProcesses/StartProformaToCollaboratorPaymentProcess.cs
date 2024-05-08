using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.CollaboratorPayments;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class StartProformaToCollaboratorPaymentProcess
{
    public class Command
    {
        public Guid CollaboratorId { get; set; }
        public Currency Currency { get; set; }
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
            var process = new ProformaToCollaboratorPaymentProcess(NewId.Next().ToSequentialGuid(), command.CollaboratorId, command.Currency, command.ProformaWeekWorkItems!.Select(p => (p.ProformaId, p.Week, p.CollaboratorId, p.Status!.ToEnum<ProformaStatus>())), command.CreatedAt);

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

        var result = await behavior.Handle(async () =>
        {
            var result = await handler.Handle(command);

            foreach (var collaborator in proformaWeekWorkItems.Items.GroupBy(i => new { i.CollaboratorId }))
            {
                await registerCollaboratorPaymentHandler.Handle(new RegisterCollaboratorPayment.Command()
                {
                    CollaboratorPaymentId = result.CollaboratorPaymentId,
                    CollaboratorId = collaborator.Key.CollaboratorId,
                    CreatedAt = command.CreatedAt,
                    GrossSalary = collaborator.Sum(i => i.GrossSalary),
                    WithholdingPercentage = collaborator.Average(i => i.WithholdingPercentage),
                    Currency = command.Currency
                });
            }

            return result;
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SearchCollaborators.Runner runner)
    {
        var result = await runner.Run(new SearchCollaborators.Query() { });

        return new RazorComponentResult<StartProformaToCollaboratorPaymentProcessPage>(new
        {
            Collaborators = result
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] RegisterCollaboratorPayment.Handler registerCollaboratorPaymentHandler,
    [FromServices] ListProformaWeekWorkItems.Runner listProformaWeekWorkItemsHandler,
    [FromServices] ListCollaboratorPayments.Runner listCollaboratorPaymentsRunner,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var register = await Handle(behavior, handler, registerCollaboratorPaymentHandler, listProformaWeekWorkItemsHandler, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collaborator payment", register.Value!.CollaboratorPaymentId);

        return await ListCollaboratorPayments.HandlePage(new ListCollaboratorPayments.Query() { }, listCollaboratorPaymentsRunner);
    }
}
