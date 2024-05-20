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

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collectionId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        command.CollectionId = collectionId;

        command.CanceledAt = clock.Now;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collection = await dbContext.Get<Collection>(command.CollectionId);

            if (collection == null)
            {
                throw new NotFoundException<Collection>();
            }

            collection.Cancel(command.CanceledAt);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] IClock clock,
    HttpContext context,
    Guid collectionId)
    {
        var command = new Command()
        {
            CollectionId = collectionId,
            CanceledAt = clock.Now
        };

        await Handle(behavior, dbContext, collectionId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("collection", "canceled", command.CollectionId);

        return await GetCollection.HandlePage(runner, command.CollectionId);
    }
}
