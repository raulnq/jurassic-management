namespace WebAPI.Proformas;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformas(this IServiceCollection services)
    {
        services
            .AddTransient<AddWorkItem.Handler>()
            .AddTransient<RemoveWorkItem.Handler>()
            .AddTransient<ListProformaWeekWorkItems.Runner>()
            .AddTransient<ListProformaWeeks.Runner>()
            .AddTransient<GetProformaWeekWorkItem.Runner>()
            .AddTransient<GetProformaWeek.Runner>()
            .AddTransient<GetProforma.Runner>();

        return services;
    }
}