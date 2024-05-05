using CollaboratorPayments;

namespace WebAPI.CollaboratorPayments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<RegisterCollaboratorPayment.Handler>();

        services.AddTransient<UploadDocument.Handler>();

        services.AddTransient<ConfirmCollaboratorPayment.Handler>();

        services.AddTransient<PayCollaboratorPayment.Handler>();

        services.AddTransient<CancelCollaboratorPayment.Handler>();

        services.AddTransient<ListCollaboratorPayments.Runner>();

        services.AddTransient<GetCollaboratorPayment.Runner>();

        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new CollaboratorPaymentStorage(connectionString));

        return services;
    }
}