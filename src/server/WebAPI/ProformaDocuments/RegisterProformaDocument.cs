using FluentValidation;
using QuestPDF.Fluent;
using Rebus.Handlers;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;
using static WebAPI.Proformas.IssueProforma;

namespace WebAPI.ProformaDocuments;

public static class RegisterProformaDocument
{
    public class Command
    {
        public Guid ProformaId { get; set; }
        public string Url { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.Url).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task Handle(Command command)
        {
            var proformaDocument = new ProformaDocument(command.ProformaId, command.Url);

            _context.Set<ProformaDocument>().Add(proformaDocument);

            return Task.CompletedTask;
        }
    }

    public class ProformaIssuedEventDispatcher : IHandleMessages<ProformaIssued>
    {
        private readonly Handler _handler;

        private readonly TransactionBehavior _behavior;

        private readonly ProformaDocumentStorage _storage;

        private readonly GetProforma.Runner _getProformaRunner;

        private readonly ListProformaWeekWorkItems.Runner _listProformaWeekWorkItemsRunner;

        public ProformaIssuedEventDispatcher(Handler handler,
            TransactionBehavior behavior,
            ProformaDocumentStorage storage,
            GetProforma.Runner getProformaRunner,
            ListProformaWeekWorkItems.Runner listProformaWeekWorkItemsRunner)
        {
            _handler = handler;
            _behavior = behavior;
            _storage = storage;
            _getProformaRunner = getProformaRunner;
            _listProformaWeekWorkItemsRunner = listProformaWeekWorkItemsRunner;
        }

        public async Task Handle(ProformaIssued message)
        {
            var url = string.Empty;

            var getProformaResult = await _getProformaRunner.Run(new GetProforma.Query() { ProformaId = message.ProformaId });

            var listProformaWeekWorkItemsResult = await _listProformaWeekWorkItemsRunner.Run(new ListProformaWeekWorkItems.Query() { ProformaId = message.ProformaId, PageSize = 500 });

            using (var stream = new MemoryStream())
            {
                var document = new ProformaPdf(getProformaResult, listProformaWeekWorkItemsResult.Items);

                document.GeneratePdf(stream);

                stream.Position = 0;

                url = await _storage.Upload($"{message.ProformaId}.pdf", stream);
            }

            var command = new Command { ProformaId = message.ProformaId, Url = url };

            new Validator().ValidateAndThrow(command);

            await _behavior.Handle(() => _handler.Handle(command));
        }
    }
}
