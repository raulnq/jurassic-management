using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorRoles;

public static class EditCollaboratorRole
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollaboratorRoleId { get; set; }
        public string? Name { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CollaboratorRoleId).NotEmpty();
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.FeeAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.ProfitPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
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
            var collaboratorRole = await _context.Get<CollaboratorRole>(command.CollaboratorRoleId);

            collaboratorRole.Edit(command.Name!, command.FeeAmount, command.ProfitPercentage);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command)
    {
        command.CollaboratorRoleId = collaboratorRoleId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] GetCollaboratorRole.Runner runner,
    [FromRoute] Guid collaboratorRoleId)
    {
        var result = await runner.Run(new GetCollaboratorRole.Query() { CollaboratorRoleId = collaboratorRoleId });

        return new RazorComponentResult<EditCollaboratorRolePage>(new { Endpoints = Endpoints.Instance, Result = result });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollaboratorRole.Runner runner,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command,
    HttpContext context)
    {
        command.CollaboratorRoleId = collaboratorRoleId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        var result = await runner.Run(new GetCollaboratorRole.Query() { CollaboratorRoleId = collaboratorRoleId });

        context.Response.Headers.TriggerShowSuccessMessage($"The collaborator role {result.CollaboratorRoleId} was updated successfully");

        return new RazorComponentResult<EditCollaboratorRolePage>(new { Endpoints = Endpoints.Instance, Result = result });
    }
}