using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class CancelProforma
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CanceledAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        command.ProformaId = proformaId;

        command.CanceledAt = clock.Now;

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

            proforma.Cancel(command.CanceledAt);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] IClock clock,
        HttpContext httpContext,
        Guid proformaId)
    {
        var command = new Command()
        {
            ProformaId = proformaId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, dbContext, proformaId, clock, command);

        httpContext.Response.Headers.TriggerShowSuccessMessage($"proforma", "canceled", proformaId);

        return await GetProforma.HandlePage(runner, dbContext, proformaId);
    }
}
