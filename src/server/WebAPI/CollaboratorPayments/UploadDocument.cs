using CollaboratorPayments;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToCollaboratorPaymentProcesses;


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
    [FromServices] CollaboratorPaymentStorage storage,
    [FromRoute] Guid collaboratorPaymentId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var ext = Path.GetExtension(file.FileName);

            var url = await storage.Upload($"{Guid.NewGuid()}{ext}".ToString(), stream);

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

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collaboratorPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            CollaboratorPaymentId = collaboratorPaymentId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] GetCollaboratorPayment.Runner getCollaboratorPaymentRunner,
    [FromServices] ListProformaToCollaboratorPaymentProcessItems.Runner listProformaToCollaboratorPaymentProcessItemsRunner,
    [FromServices] CollaboratorPaymentStorage storage,
    IFormFile file,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, handler, storage, collaboratorPaymentId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the collaborator payment", "uploaded", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(getCollaboratorPaymentRunner, listProformaToCollaboratorPaymentProcessItemsRunner, collaboratorPaymentId);
    }
}
