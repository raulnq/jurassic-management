using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class SearchProformas
{
    public class Query
    {
        public Guid? ProjectId { get; set; }
        public string? Status { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public string? Number { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Total { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) =>
            {
                var statement = qf.Query(Tables.Proformas);
                if (query.ProjectId.HasValue)
                {
                    statement = statement.Where(Tables.Proformas.Field(nameof(Proforma.ProjectId)), query.ProjectId);
                }
                if (!string.IsNullOrEmpty(query.Status))
                {
                    statement = statement.Where(Tables.Proformas.Field(nameof(Proforma.Status)), query.Status);
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

        return new RazorComponentResult<SearchProformasPage>(new { Result = result });
    }
}
