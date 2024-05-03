﻿

using Microsoft.Extensions.DependencyInjection;
using Tests.Clients;
using Tests.CollaboratorRoles;
using Tests.Collaborators;
using Tests.Collections;
using Tests.Infrastructure;
using Tests.Invoices;
using Tests.InvoiceToCollectionProcesses;
using Tests.Proformas;
using Tests.ProformaToCollaboratorPaymentProcesses;
using Tests.ProformaToInvoiceProcesses;
using Tests.Projects;
using WebAPI.Clients;
using WebAPI.Proformas;
using WebAPI.ProformaToInvoiceProcesses;

namespace Tests;

public class AppDsl : IAsyncDisposable
{
    private readonly ApplicationFactory _applicationFactory;

    public FixedClock Clock { get; set; }

    public AppDsl(Action<IServiceCollection>? ConfigureServices = null)
    {
        Clock = new FixedClock(DateTimeOffset.UtcNow);

        _applicationFactory = new ApplicationFactory
        {
            Clock = Clock,
            //ApiKeys = new ApiKeySettings { { "fake-api-key", "Api" } },
            ConfigureServices = ConfigureServices
        };

        var httpDriver = new HttpDriver(_applicationFactory, "fake-api-key");

        Collaborator = new CollaboratorDsl(httpDriver);

        Client = new ClientDsl(httpDriver);

        CollaboratorRole = new CollaboratorRoleDsl(httpDriver);

        Project = new ProjectDsl(httpDriver);

        Proformas = new ProformasDsl(httpDriver);

        ProformaToInvoiceProcess = new ProformaToInvoiceProcessDsl(httpDriver);

        Invoice = new InvoiceDsl(httpDriver);

        InvoiceToCollectionProcess = new InvoiceToCollectionProcessDsl(httpDriver);

        Collection = new CollectionDsl(httpDriver);

        ProformaToCollaboratorPaymentProcess = new ProformaToCollaboratorPaymentProcessDsl(httpDriver);

        CollaboratorPayment = new CollaboratorPaymentDsl(httpDriver);
    }

    public CollaboratorPaymentDsl CollaboratorPayment { get; set; }

    public ProformaToCollaboratorPaymentProcessDsl ProformaToCollaboratorPaymentProcess { get; set; }

    public CollectionDsl Collection { get; set; }

    public InvoiceDsl Invoice { get; set; }

    public CollaboratorDsl Collaborator { get; }

    public CollaboratorRoleDsl CollaboratorRole { get; }

    public ClientDsl Client { get; }

    public ProjectDsl Project { get; }

    public ProformasDsl Proformas { get; set; }

    public ProformaToInvoiceProcessDsl ProformaToInvoiceProcess { get; set; }

    public InvoiceToCollectionProcessDsl InvoiceToCollectionProcess { get; set; }

    public ValueTask DisposeAsync()
    {
        return _applicationFactory.DisposeAsync();
    }

    public async Task<(RegisterProforma.Result, RegisterClient.Command, RegisterProforma.Command, RegisterClient.Result)> RegisterProforma(DateTime start, int days = 6, Action<RegisterClient.Command>? clientSetup = null)
    {
        var (clientCommand, client) = await Client.Register(clientSetup);

        var (_, project) = await Project.Add(c =>
        {
            c.ClientId = client!.ClientId;
        });

        var (command, result) = await Proformas.Register(c =>
        {
            c.ProjectId = project!.ProjectId;
            c.Start = start;
            c.End = c.Start.AddDays(days);
            c.Discount = 0;
        });

        return (result!, clientCommand, command, client!);
    }

    public async Task<(RegisterProforma.Result, RegisterProforma.Command, RegisterClient.Result)> RegisterProformaReadyToIssue(DateTime start, int days = 6)
    {
        var (proformaResult, clientCommand, proformaCommand, clientResult) = await RegisterProforma(start, days);

        var (_, collaborator) = await Collaborator.Register();

        var (_, collaboratorRole) = await CollaboratorRole.Register();

        var weeks = (days + 1) / 7;

        for (var i = 1; i <= weeks; i++)
        {
            await Proformas.AddWorkItem(c =>
            {
                c.ProformaId = proformaResult!.ProformaId;
                c.Week = i;
                c.CollaboratorId = collaborator!.CollaboratorId;
                c.CollaboratorRoleId = collaboratorRole!.CollaboratorRoleId;
                c.Hours = clientCommand.PenaltyMinimumHours;
                c.FreeHours = 0;
            });
        }

        return (proformaResult, proformaCommand, clientResult);
    }

    public async Task<(RegisterProforma.Result, RegisterProforma.Command, RegisterClient.Result)> IssuedProforma(DateTime start, int days = 6)
    {
        var (proformaResult, proformaCommand, clientResult) = await RegisterProformaReadyToIssue(start, days);

        await Proformas.Issue(c =>
        {
            c.ProformaId = proformaResult.ProformaId;
            c.IssueAt = start.AddDays(days + 1);
        });

        return (proformaResult, proformaCommand, clientResult);
    }

    public async Task<StartProformaToInvoiceProcess.Result> IssuedInvoice(Guid proformaId, Guid clientId, Currency currency, DateTime today)
    {
        var (_, start) = await ProformaToInvoiceProcess.Start(c =>
        {
            c.Currency = currency;
            c.ClientId = clientId;
            c.ProformaId = new[] { proformaId };
        });

        await Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);

        await Invoice.Issue(c =>
        {
            c.InvoiceId = start!.InvoiceId;
            c.IssueAt = today.AddDays(1);
        });

        return start!;
    }
}