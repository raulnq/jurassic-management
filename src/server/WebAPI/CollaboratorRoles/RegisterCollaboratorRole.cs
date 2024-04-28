using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorRoles;

public static class RegisterCollaboratorRole
{
    public class Command
    {
        public string? Name { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorRoleId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.FeeAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.ProfitPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
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
            var collaboratorRole = new CollaboratorRole(NewId.Next().ToSequentialGuid(), command.Name!, command.FeeAmount, command.ProfitPercentage);

            _context.Set<CollaboratorRole>().Add(collaboratorRole);

            return Task.FromResult(new Result()
            {
                CollaboratorRoleId = collaboratorRole.CollaboratorRoleId
            });
        }
    }


    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterCollaboratorRolePage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListCollaboratorRoles.Runner runner,
    [FromBody] Command command,
    HttpContext context)
    {
        new Validator().ValidateAndThrow(command);

        var registerResult = await behavior.Handle(() => handler.Handle(command));

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collaborator role", registerResult.CollaboratorRoleId);

        return await ListCollaboratorRoles.HandlePage(new ListCollaboratorRoles.Query() { }, runner);
    }
}
