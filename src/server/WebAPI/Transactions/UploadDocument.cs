using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Transactions;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.Transactions;

public static class UploadDocument
{
    public class Command
    {
        public Guid TransactionId { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.DocumentUrl).NotEmpty().MaximumLength(500);
            RuleFor(command => command.TransactionId).NotEmpty();
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

            transaction.UploadDocument(command.DocumentUrl!);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] TransactionStorage storage,
    [FromRoute] Guid transactionId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var ext = Path.GetExtension(file.FileName);

            var url = await storage.Upload($"{Guid.NewGuid()}{ext}".ToString(), stream, file.ContentType);

            var command = new Command
            {
                TransactionId = transactionId,
                DocumentUrl = url
            };

            new Validator().ValidateAndThrow(command);

            await behavior.Handle(() => handler.Handle(command));

            return TypedResults.Ok();
        }
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid transactionId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            TransactionId = transactionId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetTransaction.Runner runner,
    [FromServices] TransactionStorage storage,
    IFormFile file,
    Guid transactionId,
    HttpContext context)
    {
        await Handle(behavior, handler, storage, transactionId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the transaction", "uploaded", transactionId);

        return await EditTransaction.HandlePage(runner, transactionId);
    }
}
