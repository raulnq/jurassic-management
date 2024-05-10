using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
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
    [FromServices] GetProforma.Runner getProformaRunner,
    [FromServices] TempoService tempoService,
    [FromServices] AddWorkItem.Handler addWorkItemHandler,
    [FromServices] TransactionBehavior behavior,
    [FromServices] ListProformaWeekWorkItems.Runner listProformaWeekWorkItemsRunner,
    [FromServices] RemoveWorkItem.Handler removeWorkItemHandler,
    [FromBody] Command command)
    {
        var getProformaResult = await getProformaRunner.Run(new GetProforma.Query { ProformaId = proformaId });

        var getJiraProfileProjectResult = await getJiraProfileProjectRunner.Run(new GetJiraProfileProject.Query() { ProjectId = getProformaResult.ProjectId });

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

            var listProformaWeekWorkItemsResult = await listProformaWeekWorkItemsRunner.Run(new ListProformaWeekWorkItems.Query() { ProformaId = proformaId, Week = week, PageSize = 50 });

            await behavior.Handle(async () =>
            {
                foreach (var item in listProformaWeekWorkItemsResult.Items)
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
    [FromServices] ListProformaWeekWorkItems.Runner runner,
    [FromServices] GetProforma.Runner getProformaRunner,
    [FromServices] GetProformaWeek.Runner getProformaWeekRunner,
    [FromServices] GetJiraProfileProject.Runner getJiraProfileProjectRunner,
    [FromServices] ListJiraProfileAccounts.Runner listJiraProfileAccountsRunner,
    [FromServices] TempoService tempoService,
    [FromServices] AddWorkItem.Handler addWorkItemHandler,
    [FromServices] TransactionBehavior behavior,
    [FromServices] ListProformaWeekWorkItems.Runner listProformaWeekWorkItemsRunner,
    [FromServices] RemoveWorkItem.Handler removeWorkItemHandler,
    Guid proformaId,
    int week,
    [FromBody] Command command)
    {
        await Handle(proformaId, week, getJiraProfileProjectRunner,
            listJiraProfileAccountsRunner, getProformaRunner, tempoService, addWorkItemHandler, behavior, listProformaWeekWorkItemsRunner, removeWorkItemHandler, command);

        return await ListProformaWeekWorkItems.HandlePage(
            new ListProformaWeekWorkItems.Query() { ProformaId = proformaId, Week = week },
            runner,
            getProformaRunner,
            getProformaWeekRunner,
            getJiraProfileProjectRunner,
            proformaId, week);

    }
}
