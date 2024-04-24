using FluentValidation;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;


namespace WebAPI.CollaboratorPayments;

public static class RegisterCollaboratorPayment
{
    public class Command
    {
        public decimal GrossSalary { get; set; }
        public decimal WithholdingPercentage { get; set; }
        public Guid CollaboratorPaymentId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid ProformaId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.GrossSalary).GreaterThan(0);
            RuleFor(command => command.WithholdingPercentage).GreaterThan(0).LessThanOrEqualTo(100);
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
            RuleFor(command => command.Week).GreaterThan(0);
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result> Handle(Command command)
        {
            var payment = new CollaboratorPayment(command.CollaboratorPaymentId, command.ProformaId, command.Week, command.CollaboratorId, command.GrossSalary, command.WithholdingPercentage, command.Currency, command.CreatedAt);

            _context.Set<CollaboratorPayment>().Add(payment);

            return Task.FromResult(new Result()
            {
                CollaboratorPaymentId = payment.CollaboratorPaymentId
            });
        }
    }
}
