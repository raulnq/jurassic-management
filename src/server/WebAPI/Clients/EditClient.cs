using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Projects;

namespace WebAPI.Clients;

public static class EditClient
{
    public class Command
    {
        [JsonIgnore]
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string DocumentNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public decimal PenaltyMinimumHours { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ClientId).NotEmpty();
            RuleFor(command => command.Name).MaximumLength(200).NotEmpty();
            RuleFor(command => command.PhoneNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.DocumentNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.Address).MaximumLength(500).NotEmpty();
            RuleFor(command => command.PenaltyMinimumHours).GreaterThanOrEqualTo(0);
            RuleFor(command => command.PenaltyAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.TaxesExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.AdministrativeExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.BankingExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.MinimumBankingExpenses).GreaterThanOrEqualTo(0);
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
            var client = await _context.Get<Client>(command.ClientId);

            client.Edit(command.Name!, command.PhoneNumber, command.DocumentNumber, command.Address);

            client.EditExpenses(command.TaxesExpensesPercentage, command.AdministrativeExpensesPercentage, command.BankingExpensesPercentage, command.MinimumBankingExpenses);

            client.EditPenalty(command.PenaltyMinimumHours, command.PenaltyAmount);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        command.ClientId = clientId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] GetClient.Runner getClientRunner,
    [FromServices] ListProjects.Runner listProjectsRunner,
    [FromRoute] Guid clientId)
    {
        var getClientResult = await getClientRunner.Run(new GetClient.Query() { ClientId = clientId });

        var listProjectQuery = new ListProjects.Query() { ClientId = clientId };

        var listProjectResult = await listProjectsRunner.Run(new ListProjects.Query() { ClientId = clientId });

        return new RazorComponentResult<EditClientPage>(new
        {
            GetClientResult = getClientResult,
            ListProjectsResult = listProjectResult,
            ListProjectsQuery = listProjectQuery,
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetClient.Runner getClientRunner,
    [FromServices] ListProjects.Runner listProjectsRunner,
    [FromRoute] Guid clientId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, handler, clientId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("client", command.ClientId);

        return await HandlePage(getClientRunner, listProjectsRunner, command.ClientId);
    }
}