
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Net.Mime;
using Tests.Infrastructure;
using WebAPI.CollaboratorPayments;

namespace Tests.Invoices;

public class CollaboratorPaymentDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "collaborator-payments";
    public CollaboratorPaymentDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<UploadDocument.Command?> Upload(string file, Action<UploadDocument.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<UploadDocument.Command>()
           ;
        var request = faker.Generate();

        setup?.Invoke(request);

        using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var fileName = Path.GetFileName(file);

            var (status, result, error) = await _httpDriver.Post<EmptyResult>($"{_uri}/{request.CollaboratorPaymentId}/upload-document", fs, fileName, MediaTypeNames.Application.Pdf);

            (status, error).Check(errorDetail, errors: errors);

            return request;
        }
    }

    public async Task<(RegisterCollaboratorPayment.Command, RegisterCollaboratorPayment.Result)> Register(Action<RegisterCollaboratorPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterCollaboratorPayment.Command>()
            .RuleFor(command => command.GrossSalary, faker => faker.Random.Number(100, 1000))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterCollaboratorPayment.Command, RegisterCollaboratorPayment.Result>($"{_uri}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.CollaboratorPaymentId.ShouldNotBe(Guid.Empty);
        });

        return (request, result!);
    }

    public async Task<EditCollaboratorPayment.Command> Pay(Action<EditCollaboratorPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditCollaboratorPayment.Command>()
            .RuleFor(command => command.GrossSalary, faker => faker.Random.Number(100, 1000))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{request.CollaboratorPaymentId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<PayCollaboratorPayment.Command> Pay(Action<PayCollaboratorPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<PayCollaboratorPayment.Command>()
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{request.CollaboratorPaymentId}/pay", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<ConfirmCollaboratorPayment.Command> Confirm(Action<ConfirmCollaboratorPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<ConfirmCollaboratorPayment.Command>()
            .RuleFor(command => command.Number, faker => faker.Random.Guid().ToString());

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{request.CollaboratorPaymentId}/confirm", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }
}