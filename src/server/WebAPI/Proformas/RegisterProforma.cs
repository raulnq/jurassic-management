using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Projects;

namespace WebAPI.Proformas;

public static class RegisterProforma
{
    public class Command
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Discount { get; set; }
        public Currency Currency { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonIgnore]
        public int Count { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProjectId).NotEmpty();
            RuleFor(command => command.End).GreaterThanOrEqualTo(command => command.Start);
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command command)
        {
            var project = await _context.Get<Project>(command.ProjectId);

            var client = await _context.Get<Client>(project.ClientId);

            var proforma = new Proforma(NewId.Next().ToSequentialGuid(), command.Start, command.End, command.ProjectId, client, command.CreatedAt, command.Discount, command.Currency, command.Count);

            _context.Set<Proforma>().Add(proforma);

            return new Result()
            {
                ProformaId = proforma.ProformaId
            };
        }
    }


    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] IClock clock,
    [FromServices] CountProformas.Runner runner,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        command.Count = await runner.Run(new CountProformas.Query() { End = command.End });

        command.CreatedAt = clock.Now;

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] SearchClients.Runner runner)
    {
        var result = await runner.Run(new SearchClients.Query() { });

        return new RazorComponentResult<RegisterProformaPage>(new
        {
            Clients = result
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListProformas.Runner listProformasRunner,
    [FromServices] CountProformas.Runner countProformaRunner,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var register = await Handle(behavior, handler, clock, countProformaRunner, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"proforma", register.Value!.ProformaId);

        return await ListProformas.HandlePage(new ListProformas.Query() { }, listProformasRunner);
    }
}
