using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorRoles;

public static class ListCollaboratorRoles
{
    public class Query : ListQuery
    {
        public string? Name { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorRoleId { get; set; }
        public string? Name { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.CollaboratorRoles);

                if (!string.IsNullOrEmpty(query.Name))
                {
                    statement = statement.WhereLike(Tables.CollaboratorRoles.Field(nameof(Query.Name)), $"%{query.Name}%");
                }
                return statement;
            }, query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] Runner runner,
    [AsParameters] Query query)
    {
        return TypedResults.Ok(await runner.Run(query));
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);
        return new RazorComponentResult<ListCollaboratorRolesPage>(new { Result = result, Query = query });
    }
}
