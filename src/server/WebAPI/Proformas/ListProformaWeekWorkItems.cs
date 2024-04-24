using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class ListProformaWeekWorkItems
{
    public class Query : ListQuery
    {
        public Guid? ProformaId { get; set; }
        public int? Week { get; set; }

        public IEnumerable<Guid>? ListOfProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public decimal Hours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal FreeHours { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
        public decimal WithholdingPercentage { get; set; }
        public string? Status { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.ProformaWeekWorkItems)
                .Select(Tables.ProformaWeekWorkItems.AllFields)
                .Select(Tables.Collaborators.Field(nameof(Result.WithholdingPercentage)))
                .Select(Tables.Proformas.Field(nameof(Result.Status)), Tables.Proformas.Field(nameof(Result.Currency)))
                .Join(Tables.Collaborators, Tables.ProformaWeekWorkItems.Field("CollaboratorId"), Tables.Collaborators.Field("CollaboratorId"))
                .Join(Tables.Proformas, Tables.Proformas.Field("ProformaId"), Tables.ProformaWeekWorkItems.Field("ProformaId"));

                if (query.ProformaId.HasValue)
                {
                    statement = statement
                        .Where(Tables.ProformaWeekWorkItems.Field(nameof(Query.ProformaId)), query.ProformaId);
                }

                if (query.Week.HasValue)
                {
                    statement = statement
                        .Where(Tables.ProformaWeekWorkItems.Field(nameof(Query.Week)), query.Week);
                }

                if (query.ListOfProformaId != null)
                {
                    statement = statement
                        .WhereIn(Tables.ProformaWeekWorkItems.Field(nameof(Query.ProformaId)), query.ListOfProformaId);
                }

                return statement;
            }, query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    [AsParameters] Query query)
    {
        query.ProformaId = proformaId;
        query.Week = week;
        return TypedResults.Ok(await runner.Run(query));
    }
}
