using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Collaborators;

public static class RegisterCollaborator
{
    public class Command
    {
        public string? Name { get; set; }
        public decimal WithholdingPercentage { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.WithholdingPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
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
            var collaborator = new Collaborator(NewId.Next().ToSequentialGuid(), command.Name!, command.WithholdingPercentage);

            _context.Set<Collaborator>().Add(collaborator);

            return Task.FromResult(new Result()
            {
                CollaboratorId = collaborator.CollaboratorId
            });
        }
    }


    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterCollaboratorPage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListCollaborators.Runner runner,
    [FromBody] Command command,
    HttpContext context)
    {
        var result = await Handle(behavior, handler, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collaborator", result.Value!.CollaboratorId);

        return await ListCollaborators.HandlePage(new ListCollaborators.Query() { }, runner);
    }
}
