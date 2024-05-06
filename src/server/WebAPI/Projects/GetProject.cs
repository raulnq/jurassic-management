using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Projects;

public static class GetProject
{
    public class Query
    {
        public Guid ProjectId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Projects)
                .Where(Tables.Projects.Field(nameof(Project.ProjectId)), query.ProjectId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid clientId,
    [FromRoute] Guid projectId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { ProjectId = projectId }));
    }
}
