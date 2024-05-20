using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public string Name { get; set; } = default!;
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

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext context,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command)
    {
        command.CollaboratorRoleId = collaboratorRoleId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaboratorRole = await context.Get<CollaboratorRole>(command.CollaboratorRoleId);

            collaboratorRole.Edit(command.Name!, command.FeeAmount, command.ProfitPercentage);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorRoleId)
    {
        var collaboratorRole = await dbContext.Set<CollaboratorRole>().AsNoTracking().FirstAsync(cr => cr.CollaboratorRoleId == collaboratorRoleId);

        return new RazorComponentResult<EditCollaboratorRolePage>(new { CollaboratorRole = collaboratorRole });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbcontext,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, dbcontext, collaboratorRoleId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("collaborator role", command.CollaboratorRoleId);

        return await HandlePage(dbcontext, command.CollaboratorRoleId);
    }
}