namespace WebAPI.ProformaToInvoiceProcesses;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformaToInvoiceProcesses(this IServiceCollection services)
    {
        services.AddTransient<StartProformaToInvoiceProcess.Handler>();

        services.AddTransient<ListProformaToInvoiceProcessItems.Runner>();

        return services;
    }
}