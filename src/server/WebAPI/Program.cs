using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using WebAPI;
using WebAPI.Clients;
using WebAPI.CollaboratorPayments;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Collections;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.OpenApi;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Invoices;
using WebAPI.InvoiceToCollectionProcesses;
using WebAPI.ProformaDocuments;
using WebAPI.Proformas;
using WebAPI.ProformaToCollaboratorPaymentProcesses;
using WebAPI.ProformaToInvoiceProcesses;
using WebAPI.Projects;
using WebAPI.Transactions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();
    options.CustomSchemaIds(CustomSchemaIdProvider.Get);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEntityFramework(builder.Configuration);
builder.Services.AddSqlKata(builder.Configuration);
builder.Services.AddRebus(builder.Configuration, onCreated: bus =>
{
    return bus.SubscribeToProforma();
});
builder.Services.AddCollaboratorRoles();
builder.Services.AddCollaborators();
builder.Services.AddProjects();
builder.Services.AddClients();
builder.Services.AddProblemDetails();
builder.Services.AddProformas();
builder.Services.AddProformaToInvoiceProcesses();
builder.Services.AddInvoices(builder.Configuration);
builder.Services.AddInvoiceToCollectionProcesses();
builder.Services.AddCollections();
builder.Services.AddProformaToCollaboratorPaymentProcesses();
builder.Services.AddCollaboratorPayments(builder.Configuration);
builder.Services.AddProformaDocuments(builder.Configuration);
builder.Services.AddTransactions(builder.Configuration);
builder.Services.AddSingleton<IClock>(new SystemClock());
builder.Services.AddExceptionHandler<DefaultExceptionHandler>();
builder.Services.AddRazorComponents();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ui", () =>
{
    return new RazorComponentResult<Main>();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseExceptionHandler();
app.RegisterCollaboratorRoleEndpoints();
app.RegisterCollaboratorEndpoints();
app.RegisterProjectEndpoints();
app.RegisterClientEndpoints();
app.RegisterProformaEndpoints();
app.RegisterProformaToInvoiceProcessEndpoints();
app.RegisterInvoiceEndpoints();
app.RegisterInvoiceToCollectionProcessEndpoints();
app.RegisterCollectionEndpoints();
app.RegisterProformaToColaboratorPaymentProcessEndpoints();
app.RegisterCollaboratorPaymentEndpoints();
app.RegisterTransactionEndpoints();
app.Run();