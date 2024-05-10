using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;


namespace WebAPI.CollaboratorPayments;

public static class RegisterCollaboratorPayment
{
    public class Command
    {
        public decimal GrossSalary { get; set; }
        [JsonIgnore]
        public decimal WithholdingPercentage { get; set; }
        [JsonIgnore]
        public Guid CollaboratorPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.GrossSalary).GreaterThan(0);
            RuleFor(command => command.WithholdingPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
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
            var payment = new CollaboratorPayment(command.CollaboratorPaymentId, command.CollaboratorId, command.GrossSalary, command.WithholdingPercentage, command.Currency, command.CreatedAt);

            _context.Set<CollaboratorPayment>().Add(payment);

            return Task.FromResult(new Result()
            {
                CollaboratorPaymentId = payment.CollaboratorPaymentId
            });
        }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] Handler handler,
        [FromServices] GetCollaborator.Runner getCollaboratorRunner,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        command.CreatedAt = clock.Now;

        command.CollaboratorPaymentId = NewId.Next().ToSequentialGuid();

        var collaboratorResult = await getCollaboratorRunner.Run(new GetCollaborator.Query() { CollaboratorId = command.CollaboratorId });

        command.WithholdingPercentage = collaboratorResult.WithholdingPercentage;

        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] SearchCollaborators.Runner searchCollaboratorsRunner)
    {
        var searchCollaboratorsResult = await searchCollaboratorsRunner.Run(new SearchCollaborators.Query());

        return new RazorComponentResult<RegisterCollaboratorPaymentPage>(new
        {
            SearchCollaboratorsResult = searchCollaboratorsResult
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] Handler handler,
        [FromServices] GetCollaborator.Runner getCollaboratorRunner,
        [FromServices] ListCollaboratorPayments.Runner listCollaboratorPaymentsRunner,
        [FromServices] IClock clock,
        [FromBody] Command command,
        HttpContext context)
    {
        var result = await Handle(behavior, handler, getCollaboratorRunner, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage("collaborator payment", result.Value!.CollaboratorPaymentId);

        return await ListCollaboratorPayments.HandlePage(new ListCollaboratorPayments.Query(), listCollaboratorPaymentsRunner);
    }
}
