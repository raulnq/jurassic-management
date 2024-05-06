using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;
using WebAPI.InvoiceToCollectionProcesses;

namespace WebAPI.Collections;

public static class CancelCollection
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollectionId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CanceledAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CollectionId).NotEmpty();
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
            var collection = await _context.Get<Collection>(command.CollectionId);

            if (collection == null)
            {
                throw new NotFoundException<Collection>();
            }

            collection.Cancel(command.CanceledAt);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collectionId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.CollectionId = collectionId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollection.Runner getCollectionRunner,
    [FromServices] ListInvoiceToCollectionProcessItems.Runner listInvoiceToCollectionProcessItemsRunner,
    [FromServices] IClock clock,
    HttpContext context,
    Guid collectionId)
    {
        var command = new Command()
        {
            CollectionId = collectionId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, handler, collectionId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("collection", "canceled", command.CollectionId);

        return await GetCollection.HandlePage(getCollectionRunner, listInvoiceToCollectionProcessItemsRunner, command.CollectionId);
    }
}
