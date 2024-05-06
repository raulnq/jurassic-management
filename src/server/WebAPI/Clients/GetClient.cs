using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Clients;

public static class GetClient
{
    public class Query
    {
        public Guid ClientId { get; set; }
    }

    public class Result
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string DocumentNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public decimal PenaltyMinimumHours { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Clients)
                .Where(Tables.Clients.Field(nameof(Client.ClientId)), query.ClientId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid clientId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { ClientId = clientId }));
    }
}
