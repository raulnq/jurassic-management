using Transactions;

namespace WebAPI.Transactions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<ListTransactions.Runner>()
            .AddTransient<GetTransaction.Runner>()
            .AddTransient<RegisterTransaction.Handler>()
            .AddTransient<EditTransaction.Handler>()
            .AddTransient<UploadDocument.Handler>();

        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new TransactionStorage(connectionString));

        return services;
    }
}