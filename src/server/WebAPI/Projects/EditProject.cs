using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Projects;

public static class EditProject
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProjectId { get; set; }
        public string? Name { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProjectId).NotEmpty();
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

        public async Task Handle(Command command)
        {
            var project = await _context.Get<Project>(command.ProjectId);

            project.Edit(command.Name!);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid projectId,
    [FromBody] Command command)
    {
        command.ProjectId = projectId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        Guid clientId,
        Guid projectId,
        GetProject.Runner runner,
        HttpContext context)
    {
        var result = await runner.Run(new GetProject.Query() { ProjectId = projectId });

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditProjectPage>(new { Result = result });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] Handler handler,
        [FromServices] ListProjects.Runner runner,
        [FromBody] Command command,
        Guid clientId,
        Guid projectId,
        HttpContext context)
    {
        command.ProjectId = projectId;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        var query = new ListProjects.Query() { ClientId = clientId };

        var listResult = await runner.Run(query);

        context.Response.Headers.TriggerShowEditSuccessMessageAndCloseModal("project", command.ProjectId);

        return await ListProjects.HandlePage(new ListProjects.Query() { ClientId = clientId }, runner);
    }
}