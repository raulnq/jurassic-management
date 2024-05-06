namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformaToCollaboratorPaymentProcesses(this IServiceCollection services)
    {
        services
            .AddTransient<StartProformaToCollaboratorPaymentProcess.Handler>()
            .AddTransient<ListProformaToCollaboratorPaymentProcessItems.Runner>();

        return services;
    }
}