using FluentValidation;
using Invoices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;


namespace WebAPI.CollaboratorPayments;

public static class UploadDocument
{
    public class Command
    {
        public Guid CollaboratorPaymentId { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.DocumentUrl).NotEmpty().MaximumLength(500);
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
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
            var payment = await _context.Get<CollaboratorPayment>(command.CollaboratorPaymentId);

            payment.Upload(command.DocumentUrl!);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] InvoiceStorage storage,
    [FromRoute] Guid collaboratorPaymentId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var url = await storage.Upload(Guid.NewGuid().ToString(), stream);

            var command = new Command
            {
                CollaboratorPaymentId = collaboratorPaymentId,
                DocumentUrl = url
            };

            new Validator().ValidateAndThrow(command);

            await behavior.Handle(() => handler.Handle(command));

            return TypedResults.Ok();
        }
    }
}
