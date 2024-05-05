using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.Transactions;

public static class EditTransaction
{
    public class Command
    {
        [JsonIgnore]
        public Guid TransactionId { get; set; }
        public TransactionType Type { get; set; }
        public Currency Currency { get; set; }
        public string Description { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Number { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Description).MaximumLength(1000).NotEmpty();
            RuleFor(command => command.Number).MaximumLength(50);
            RuleFor(command => command.SubTotal).GreaterThan(0);
            RuleFor(command => command.Taxes).GreaterThanOrEqualTo(0);
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
            var transaction = await _context.Get<Transaction>(command.TransactionId);

            transaction.Edit(command.Type,
                command.Description,
                command.SubTotal,
                command.Taxes,
                command.Currency,
                command.Number,
                command.IssuedAt);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid transactionId,
    [FromBody] Command command)
    {
        command.TransactionId = transactionId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] GetTransaction.Runner runner,
    [FromRoute] Guid transactionId)
    {
        var result = await runner.Run(new GetTransaction.Query() { TransactionId = transactionId });

        return new RazorComponentResult<EditTransactionPage>(new { Result = result });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetTransaction.Runner runner,
    [FromRoute] Guid transactionId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, handler, transactionId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage($"transaction", command.TransactionId);

        return await HandlePage(runner, command.TransactionId);
    }
}