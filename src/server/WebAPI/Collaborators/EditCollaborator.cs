using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Collaborators;

public static class EditCollaborator
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollaboratorId { get; set; }
        public string? Name { get; set; }
        public decimal WithholdingPercentage { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CollaboratorId).NotEmpty();
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

        public async Task Handle(Command command)
        {
            var collaborator = await _context.Get<Collaborator>(command.CollaboratorId);

            collaborator.Edit(command.Name!, command.WithholdingPercentage);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collaboratorId,
    [FromBody] Command command)
    {
        command.CollaboratorId = collaboratorId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] GetCollaborator.Runner runner,
    [FromRoute] Guid collaboratorId)
    {
        var result = await runner.Run(new GetCollaborator.Query() { CollaboratorId = collaboratorId });

        return new RazorComponentResult<EditCollaboratorPage>(new { Result = result });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollaborator.Runner runner,
    [FromRoute] Guid collaboratorId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, handler, collaboratorId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage($"collaborator", command.CollaboratorId);

        return await HandlePage(runner, command.CollaboratorId);
    }
}