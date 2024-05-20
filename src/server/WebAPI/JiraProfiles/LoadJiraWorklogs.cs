using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace WebAPI.JiraProfiles;

public static class LoadJiraWorklogs
{
    public class Command
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public static async Task<Ok> Handle(
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    [FromServices] GetJiraProfileProject.Runner getJiraProfileProjectRunner,
    [FromServices] ListJiraProfileAccounts.Runner listJiraProfileAccountsRunner,
    [FromServices] TempoService tempoService,
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command)
    {
        var proform = await dbContext.Set<Proforma>().AsNoTracking().FirstAsync(p => p.ProformaId == proformaId);

        var getJiraProfileProjectResult = await getJiraProfileProjectRunner.Run(new GetJiraProfileProject.Query() { ProjectId = proform.ProjectId });

        if (getJiraProfileProjectResult != null)
        {
            var worklogs = await tempoService.Get(new TempoService.Request()
            {
                Start = command.Start,
                End = command.End,
                ProjectId = getJiraProfileProjectResult.JiraProjectId,
                Token = getJiraProfileProjectResult.TempoToken
            });

            var listJiraProfileAccountsResult = await listJiraProfileAccountsRunner.Run(new ListJiraProfileAccounts.Query() { ClientId = getJiraProfileProjectResult.ClientId });

            var proformaWeelWorkItems = await dbContext.Set<ProformaWeekWorkItem>().AsNoTracking().Where(i => i.ProformaId == proformaId && i.Week == week).ToListAsync();

            var addWorkItemHandler = new AddWorkItem.Handler(dbContext);

            var removeWorkItemHandler = new RemoveWorkItem.Handler(dbContext);

            await behavior.Handle(async () =>
            {
                foreach (var item in proformaWeelWorkItems)
                {
                    await removeWorkItemHandler.Handle(new RemoveWorkItem.Command() { CollaboratorId = item.CollaboratorId, ProformaId = item.ProformaId, Week = item.Week });
                }

                foreach (var account in listJiraProfileAccountsResult)
                {
                    var items = worklogs.Results.Where(worklog => worklog.Author?.AccountId == account.JiraAccountId);

                    decimal hours = items.Sum(item => Math.Round(((decimal)item.BillableSeconds) / 3600, 2, MidpointRounding.AwayFromZero));

                    if (hours > 0)
                    {
                        await addWorkItemHandler.Handle(new AddWorkItem.Command()
                        {
                            CollaboratorId = account.CollaboratorId,
                            FreeHours = 0,
                            Hours = Math.Round(hours, 4, MidpointRounding.AwayFromZero),
                            Week = week,
                            ProformaId = proformaId,
                            CollaboratorRoleId = account.CollaboratorRoleId
                        });
                    }
                }
            });
        }
        else
        {
            throw new InfrastructureException("missing-jira-profile");
        }

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] GetJiraProfileProject.Runner getJiraProfileProjectRunner,
    [FromServices] ListJiraProfileAccounts.Runner listJiraProfileAccountsRunner,
    [FromServices] TempoService tempoService,
    [FromServices] TransactionBehavior behavior,
    Guid proformaId,
    int week,
    [FromBody] Command command)
    {
        await Handle(proformaId, week, getJiraProfileProjectRunner,
            listJiraProfileAccountsRunner, tempoService, behavior, dbContext, command);

        return await ListProformaWeekWorkItems.HandlePage(
            new ListProformaWeekWorkItems.Query() { ProformaId = proformaId, Week = week },
            runner,
            dbContext,
            proformaId, week);

    }
}
