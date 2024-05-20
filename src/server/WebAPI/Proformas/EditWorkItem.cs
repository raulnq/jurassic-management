using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class EditWorkItem
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        [JsonIgnore]
        public int Week { get; set; }
        [JsonIgnore]
        public Guid CollaboratorId { get; set; }
        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
            RuleFor(command => command.Week).GreaterThan(0);
            RuleFor(command => command.Hours).GreaterThan(0);
            RuleFor(command => command.FreeHours).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command)
    {
        command.ProformaId = proformaId;

        command.Week = week;

        command.CollaboratorId = collaboratorId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var proforma = await dbContext.Set<Proforma>()
            .Include(p => p.Weeks)
            .ThenInclude(w => w.WorkItems)
            .SingleOrDefaultAsync(p => p.ProformaId == command.ProformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            proforma.EditWorkItem(command.Week, command.CollaboratorId, command.Hours, command.FreeHours);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        Guid proformaId,
        int week,
        Guid collaboratorId,
        GetProformaWeekWorkItem.Runner runner,
        HttpContext context)
    {
        var result = await runner.Run(new GetProformaWeekWorkItem.Query() { ProformaId = proformaId, Week = week, CollaboratorId = collaboratorId });

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditWorkItemPage>(new { Result = result });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ListProformaWeekWorkItems.Runner runner,
        [FromBody] Command command,
        Guid proformaId,
        int week,
        Guid collaboratorId,
        HttpContext httpContext)
    {
        await Handle(behavior, dbContext, proformaId, week, collaboratorId, command);

        httpContext.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collaborator", "edited", collaboratorId);

        return await ListProformaWeekWorkItems.HandlePage(
            new ListProformaWeekWorkItems.Query() { ProformaId = command.ProformaId, Week = command.Week },
            runner,
            dbContext,
            proformaId,
            week);

    }
}
