namespace WebAPI.InvoiceToCollectionProcesses;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoiceToCollectionProcesses(this IServiceCollection services)
    {
        services
            .AddTransient<ListInvoiceToCollectionProcessItems.Runner>();

        return services;
    }
}