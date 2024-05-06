using CollaboratorPayments;

namespace WebAPI.CollaboratorPayments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTransient<RegisterCollaboratorPayment.Handler>()
            .AddTransient<UploadDocument.Handler>()
            .AddTransient<ConfirmCollaboratorPayment.Handler>()
            .AddTransient<PayCollaboratorPayment.Handler>()
            .AddTransient<CancelCollaboratorPayment.Handler>()
            .AddTransient<ListCollaboratorPayments.Runner>()
            .AddTransient<GetCollaboratorPayment.Runner>();

        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new CollaboratorPaymentStorage(connectionString));

        return services;
    }
}