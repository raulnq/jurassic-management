using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Clients;

public static class SearchClients
{
    public class Query
    {
    }

    public class Result
    {
        public Guid ClientId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Address { get; set; }
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

        public Task<List<Result>> Run(Query query)
        {
            return _queryRunner.List<Result>((qf) => qf.Query(Tables.Clients));
        }
    }
}
