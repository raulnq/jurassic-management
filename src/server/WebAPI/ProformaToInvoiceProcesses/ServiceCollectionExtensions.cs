namespace WebAPI.ProformaToInvoiceProcesses;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformaToInvoiceProcesses(this IServiceCollection services)
    {
        services
            .AddTransient<ListProformaToInvoiceProcessItems.Runner>();

        return services;
    }
}