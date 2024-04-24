using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class ListProformas
{
    public class Query : ListQuery
    {
        public ProformaStatus? Status { get; set; }
        public IEnumerable<Guid>? ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Commission { get; set; }
        public decimal Discount { get; set; }
        public decimal MinimumHours { get; set; }
        public decimal PenaltyMinimumHours { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
        public decimal TaxesExpensesAmount { get; set; }
        public decimal AdministrativeExpensesAmount { get; set; }
        public decimal BankingExpensesAmount { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.Proformas);
                if (query.Status.HasValue)
                {
                    statement = statement.Where(Tables.Proformas.Field(nameof(query.Status)), query.Status.Value.ToString());
                }
                if (query.ProformaId != null)
                {
                    statement = statement.WhereIn(Tables.Proformas.Field(nameof(query.ProformaId)), query.ProformaId);
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
}
