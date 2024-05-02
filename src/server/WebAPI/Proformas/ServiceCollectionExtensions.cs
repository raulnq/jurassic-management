namespace WebAPI.Proformas;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformas(this IServiceCollection services)
    {
        services.AddTransient<RegisterProforma.Handler>();

        services.AddTransient<AddWorkItem.Handler>();

        services.AddTransient<EditWorkItem.Handler>();

        services.AddTransient<RemoveWorkItem.Handler>();

        services.AddTransient<IssueProforma.Handler>();

        services.AddTransient<ListProformas.Runner>();

        services.AddTransient<ListProformaWeekWorkItems.Runner>();

        services.AddTransient<ListProformaWeeks.Runner>();

        services.AddTransient<GetProformaWeekWorkItem.Runner>();

        services.AddTransient<GetProformaWeek.Runner>();

        services.AddTransient<GetProforma.Runner>();

        services.AddTransient<CountProformas.Runner>();

        services.AddTransient<MarkProformaAsInvoiced.Handler>();

        services.AddTransient<CancelProforma.Handler>();

        services.AddTransient<SearchProformas.Runner>();

        return services;
    }
}