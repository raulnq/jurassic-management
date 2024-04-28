using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Projects;

public static class SearchProjects
{
    public class Query
    {
        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string? Name { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.Projects);
                if (query.ClientId.HasValue)
                {
                    statement = statement.Where(Tables.Projects.Field(nameof(Project.ClientId)), query.ClientId);
                }
                return statement;
            });
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] Runner runner)
    {
        var result = await runner.Run(query);

        return new RazorComponentResult<SearchProjectsPage>(new { Result = result });
    }
}
