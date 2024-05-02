using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaDocuments;

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

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command command)
        {
            var proforma = await _context.Set<Proforma>()
                .Include(p => p.Weeks)
                .ThenInclude(w => w.WorkItems)
                .SingleOrDefaultAsync(p => p.ProformaId == command.ProformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            proforma.Cancel(command.CanceledAt);
        }
    }

    public class ProformaIssued
    {
        public Guid ProformaId { get; set; }
    }


    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid proformaId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.ProformaId = proformaId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetProforma.Runner getProformaRunner,
    [FromServices] ListProformaWeeks.Runner listProformasWeeksRunner,
    [FromServices] GetProformaDocument.Runner getProformaDocumentRunner,
    [FromServices] IClock clock,
    HttpContext context,
    Guid proformaId)
    {
        var command = new Command()
        {
            ProformaId = proformaId,
            CanceledAt = clock.Now
        };

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        context.Response.Headers.TriggerShowSuccessMessage($"The proforma {proformaId} was cancel successfully");

        return await GetProforma.HandlePage(getProformaRunner, listProformasWeeksRunner, getProformaDocumentRunner, command.ProformaId);
    }
}
