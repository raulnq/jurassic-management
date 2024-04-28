using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collaborators;

public static class GetCollaborator
{
    public class Query
    {
        public Guid CollaboratorId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
        public string? Name { get; set; }
        public decimal WithholdingPercentage { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Collaborators)
                .Where(Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)), query.CollaboratorId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid collaboratorId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { CollaboratorId = collaboratorId }));
    }
}
