using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Transactions;

public static class GetTransaction
{
    public class Query
    {
        public Guid TransactionId { get; set; }
    }

    public class Result
    {
        public Guid TransactionId { get; set; }
        public string? Description { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public string? Number { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Type { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Transactions)
                .Where(Tables.Transactions.Field(nameof(Transaction.TransactionId)), query.TransactionId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] Runner runner,
    [FromRoute] Guid transactionId)
    {
        return TypedResults.Ok(await runner.Run(new Query() { TransactionId = transactionId }));
    }
}
