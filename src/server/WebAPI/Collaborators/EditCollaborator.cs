using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public string Name { get; set; } = default!;
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

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command)
    {
        command.CollaboratorId = collaboratorId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaborator = await dbContext.Get<Collaborator>(command.CollaboratorId);

            collaborator.Edit(command.Name, command.WithholdingPercentage);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId)
    {
        var collaborator = await dbContext.Set<Collaborator>().AsNoTracking().FirstAsync(cr => cr.CollaboratorId == collaboratorId);

        return new RazorComponentResult<EditCollaboratorPage>(new { Collaborator = collaborator });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command,
        HttpContext context)
    {
        await Handle(behavior, dbContext, collaboratorId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("collaborator", command.CollaboratorId);

        return await HandlePage(dbContext, command.CollaboratorId);
    }
}