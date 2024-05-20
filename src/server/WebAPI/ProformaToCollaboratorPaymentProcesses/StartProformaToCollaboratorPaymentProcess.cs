using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.CollaboratorPayments;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] IClock clock,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        command.CreatedAt = clock.Now;

        var proformas = await dbContext.Set<Proforma>().AsNoTracking().Include(p => p.Weeks).ThenInclude(w => w.WorkItems).AsNoTracking().Where(i => command.ProformaId!.Contains(i.ProformaId)).ToListAsync();

        var registerCollaboratorPaymentHandler = new RegisterCollaboratorPayment.Handler(dbContext);

        var result = await behavior.Handle(async () =>
        {
            var process = new ProformaToCollaboratorPaymentProcess(NewId.Next().ToSequentialGuid(), command.CollaboratorId, command.Currency, proformas, command.CreatedAt);

            dbContext.Set<ProformaToCollaboratorPaymentProcess>().Add(process);

            foreach (var collaborator in proformas.SelectMany(p => p.Weeks).SelectMany(w => w.WorkItems).GroupBy(i => new { i.CollaboratorId }))
            {
                await registerCollaboratorPaymentHandler.Handle(new RegisterCollaboratorPayment.Command()
                {
                    CollaboratorPaymentId = process.CollaboratorPaymentId,
                    CollaboratorId = collaborator.Key.CollaboratorId,
                    CreatedAt = command.CreatedAt,
                    GrossSalary = collaborator.Sum(i => i.GrossSalary),
                    Currency = command.Currency
                });
            }

            return new Result()
            {
                CollaboratorPaymentId = process.CollaboratorPaymentId
            };
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext)
    {
        var collaborators = await dbContext.Set<Collaborator>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<StartProformaToCollaboratorPaymentProcessPage>(new
        {
            Collaborators = collaborators
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var register = await Handle(behavior, clock, dbContext, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collaborator payment", register.Value!.CollaboratorPaymentId);

        return await ListCollaboratorPayments.HandlePage(new ListCollaboratorPayments.Query() { }, runner);
    }
}
