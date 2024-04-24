using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;


namespace WebAPI.Collections;

public static class ConfirmCollection
{
    public class Command
    {
        [JsonIgnore]
        public Guid CollectionId { get; set; }
        public decimal Total { get; set; }
        public DateTime ConfirmedAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CollectionId).NotEmpty();
            RuleFor(command => command.Total).GreaterThan(0);
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

            collection.Confirm(command.Total, command.ConfirmedAt);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid collectionId,
    [FromBody] Command command)
    {
        command.CollectionId = collectionId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }
}