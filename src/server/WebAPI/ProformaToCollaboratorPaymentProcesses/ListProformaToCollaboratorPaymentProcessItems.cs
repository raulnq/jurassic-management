using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;
using WebAPI.Projects;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class ListProformaToCollaboratorPaymentProcessItems
{
    public class Query : ListQuery
    {
        public Guid? CollaboratorPaymentId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }

        public string? ProformaProjectName { get; set; }

        public string? ProformaNumber { get; set; }
        public string? ProformaCurrency { get; set; }


        public DateTime ProformaWeekStart { get; set; }
        public DateTime ProformaWeekEnd { get; set; }

        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.ProformaToCollaboratorPaymentProcessItems)
                .Select(Tables.ProformaToCollaboratorPaymentProcessItems.AllFields)
                .Select(Tables.Projects.Field(nameof(Project.Name), nameof(Result.ProformaProjectName)))
                .Select(
                    Tables.Proformas.Field(nameof(Proforma.Number), nameof(Result.ProformaNumber)),
                    Tables.Proformas.Field(nameof(Proforma.Currency), nameof(Result.ProformaCurrency))
                    )
                .Select(
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.End), nameof(Result.ProformaWeekEnd)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Start), nameof(Result.ProformaWeekStart))
                    )
                .Select(
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Hours)),
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.FeeAmount)),
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.SubTotal)),
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProfitAmount)),
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProfitPercentage)),
                    Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.FreeHours))
                    )
                .Join(Tables.Proformas, Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.ProformaId)), Tables.Proformas.Field(nameof(Proforma.ProformaId)))
                .Join(Tables.Projects, Tables.Proformas.Field(nameof(Proforma.ProjectId)), Tables.Projects.Field(nameof(Project.ProjectId)))
                .Join(Tables.ProformaWeeks, j =>
                {
                    return j.On(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.ProformaId)), Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)))
                        .On(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.Week)), Tables.ProformaWeeks.Field(nameof(ProformaWeek.Week)));
                })
                .Join(Tables.ProformaWeekWorkItems, j =>
                {
                    return j.On(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.ProformaId)), Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)))
                        .On(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.Week)), Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Week)))
                        .On(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.CollaboratorId)), Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.CollaboratorId)))
                        ;
                })
                .Where(Tables.ProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.CollaboratorPaymentId)), query.CollaboratorPaymentId), query);
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);
        return new RazorComponentResult<ListProformaToCollaboratorPaymentProcessItemsPage>(new { Result = result, Query = query });
    }
}
