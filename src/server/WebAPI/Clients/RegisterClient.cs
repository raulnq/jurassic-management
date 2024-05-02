﻿using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Clients;

public static class RegisterClient
{
    public class Command
    {
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

    public class Result
    {
        public Guid ClientId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(200).NotEmpty();
            RuleFor(command => command.PhoneNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.DocumentNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.Address).MaximumLength(500).NotEmpty();
            RuleFor(command => command.PenaltyMinimumHours).GreaterThanOrEqualTo(0);
            RuleFor(command => command.PenaltyAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.TaxesExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.AdministrativeExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.BankingExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.MinimumBankingExpenses).GreaterThanOrEqualTo(0);
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result> Handle(Command command)
        {
            var client = new Client(NewId.Next().ToSequentialGuid(),
                command.Name!,
                command.PhoneNumber!,
                command.DocumentNumber!,
                command.Address!,
                command.PenaltyMinimumHours,
                command.PenaltyAmount,
                command.TaxesExpensesPercentage,
                command.AdministrativeExpensesPercentage,
                command.BankingExpensesPercentage,
                command.MinimumBankingExpenses);

            _context.Set<Client>().Add(client);

            return Task.FromResult(new Result()
            {
                ClientId = client.ClientId
            });
        }
    }


    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterClientPage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] Handler handler,
    [FromServices] ListClients.Runner runner,
    [FromBody] Command command,
    HttpContext context)
    {
        var result = await Handle(behavior, handler, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage("client", result.Value!.ClientId);

        return await ListClients.HandlePage(new ListClients.Query(), runner);
    }
}