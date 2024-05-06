using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Projects;

public static class AddProject
{
    public class Command
    {
        [JsonIgnore]
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
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
            var project = new Project(NewId.Next().ToSequentialGuid(), command.ClientId, command.Name!);

            _context.Set<Project>().Add(project);

            return Task.FromResult(new Result()
            {
                ProjectId = project.ProjectId
            });
        }
    }


    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        command.ClientId = clientId;

        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage(Guid clientId, HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<AddProjectPage>(new { ClientId = clientId }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] Handler handler,
        [FromServices] ListProjects.Runner runner,
        [FromBody] Command command,
        Guid clientId,
        HttpContext context)
    {
        var result = await Handle(behavior, handler, clientId, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessageAndCloseModal("project", result.Value!.ProjectId);

        return await ListProjects.HandlePage(new ListProjects.Query() { ClientId = command.ClientId }, clientId, runner);
    }

}
