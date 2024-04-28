﻿using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class AddWorkItem
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        [JsonIgnore]
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
            RuleFor(command => command.CollaboratorRoleId).NotEmpty();
            RuleFor(command => command.Week).GreaterThan(0);
            RuleFor(command => command.Hours).GreaterThan(0);
            RuleFor(command => command.FreeHours).GreaterThanOrEqualTo(0);
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
            var proforma = await _context.Set<Proforma>()
                .Include(p => p.Weeks)
                .ThenInclude(w => w.WorkItems)
                .SingleOrDefaultAsync(p => p.ProformaId == command.ProformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            var collaborator = await _context.Get<Collaborator>(command.CollaboratorId);

            var collaboratorRole = await _context.Get<CollaboratorRole>(command.CollaboratorRoleId);

            proforma.AddWorkItem(command.Week, command.CollaboratorId, collaboratorRole, command.Hours, command.FreeHours);
        }
    }


    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    [FromBody] Command command)
    {
        command.ProformaId = proformaId;
        command.Week = week;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SearchCollaboratorRoles.Runner searchCollaboratorRolesRunner,
    [FromServices] SearchCollaborators.Runner searchCollaboratorsRunner,
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        var searchCollaboratorRolesResult = await searchCollaboratorRolesRunner.Run(new SearchCollaboratorRoles.Query());

        var searchCollaboratorsResult = await searchCollaboratorsRunner.Run(new SearchCollaborators.Query());

        return new RazorComponentResult<AddWorkItemPage>(new
        {
            ProformaId = proformaId,
            Week = week,
            SearchCollaboratorRolesResult = searchCollaboratorRolesResult,
            SearchCollaboratorsResult = searchCollaboratorsResult
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListProformaWeekWorkItems.Runner runner,
    [FromServices] GetProforma.Runner getProformaRunner,
    [FromServices] GetProformaWeek.Runner getProformaWeekRunner,
    [FromBody] Command command,
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    HttpContext context)
    {
        command.ProformaId = proformaId;

        command.Week = week;

        new Validator().ValidateAndThrow(command);

        await behavior.Handle(() => handler.Handle(command));

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal($"The collaborator {command.CollaboratorId} was added successfully");

        return await ListProformaWeekWorkItems.HandlePage(new ListProformaWeekWorkItems.Query() { ProformaId = command.ProformaId, Week = command.Week },
            runner, getProformaRunner, getProformaWeekRunner, proformaId, week);
    }

}
