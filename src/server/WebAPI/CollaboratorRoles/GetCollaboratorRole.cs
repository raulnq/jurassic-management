using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorRoles;

public static class GetCollaboratorRole
{
    public class Query
    {
        public Guid CollaboratorRoleId { get; set; }
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

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.CollaboratorRoles)
                .Where(Tables.CollaboratorRoles.Field(nameof(Query.CollaboratorRoleId)), query.CollaboratorRoleId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid collaboratorRoleId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { CollaboratorRoleId = collaboratorRoleId }));
    }
}
