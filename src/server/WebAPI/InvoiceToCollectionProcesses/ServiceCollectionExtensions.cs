namespace WebAPI.InvoiceToCollectionProcesses;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoiceToCollectionProcesses(this IServiceCollection services)
    {
        services
            .AddTransient<StartInvoiceToCollectionProcess.Handler>()
            .AddTransient<ListInvoiceToCollectionProcessItems.Runner>();

        return services;
    }
}