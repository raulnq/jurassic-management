using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaDocuments;

namespace WebAPI.Proformas;

public static class IssueProforma
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        public DateTimeOffset IssueAt { get; set; }
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

            proforma.Issue(command.IssueAt);
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
    [FromServices] IBus bus,
    [FromBody] Command command)
    {
        command.ProformaId = proformaId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        await bus.Publish(new ProformaIssued() { ProformaId = command.ProformaId });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid proformaId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<IssueProformaPage>(new
        {
            ProformaId = proformaId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetProforma.Runner getProformaRunner,
    [FromServices] ListProformaWeeks.Runner listProformasWeeksRunner,
    [FromServices] GetProformaDocument.Runner getProformaDocumentRunner,
    [FromServices] IBus bus,
    [FromBody] Command command,
    Guid proformaId,
    HttpContext context)
    {
        await Handle(behavior, handler, proformaId, bus, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("proforma", "issued", proformaId);

        return await GetProforma.HandlePage(getProformaRunner, listProformasWeeksRunner, getProformaDocumentRunner, command.ProformaId);
    }
}
