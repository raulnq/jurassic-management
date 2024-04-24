﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Invoices;

public static class ListInvoices
{
    public class Query : ListQuery
    {
        public InvoiceStatus? Status { get; set; }
        public IEnumerable<Guid>? InvoiceId { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? IssueAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Status { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.Invoices);

                if (query.Status.HasValue)
                {
                    statement = statement.Where(Tables.Invoices.Field(nameof(Query.Status)), query.Status.ToString());
                }
                if (query.InvoiceId != null)
                {
                    statement = statement.WhereIn(Tables.Invoices.Field(nameof(query.InvoiceId)), query.InvoiceId);
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
