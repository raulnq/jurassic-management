﻿namespace WebAPI.ProformaDocuments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformaDocuments(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services.AddTransient<RegisterProformaDocument.Handler>();

        services.AddTransient<GetProformaDocument.Runner>();

        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new ProformaDocumentStorage(connectionString));

        return services;
    }
}