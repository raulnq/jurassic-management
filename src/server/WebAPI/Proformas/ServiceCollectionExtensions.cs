namespace WebAPI.Proformas;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformas(this IServiceCollection services)
    {
        services.AddTransient<RegisterProforma.Handler>()
            .AddTransient<AddWorkItem.Handler>()
            .AddTransient<EditWorkItem.Handler>()
            .AddTransient<RemoveWorkItem.Handler>()
            .AddTransient<IssueProforma.Handler>()
            .AddTransient<ListProformas.Runner>()
            .AddTransient<ListProformaWeekWorkItems.Runner>()
            .AddTransient<ListProformaWeeks.Runner>()
            .AddTransient<GetProformaWeekWorkItem.Runner>()
            .AddTransient<GetProformaWeek.Runner>()
            .AddTransient<GetProforma.Runner>()
            .AddTransient<CountProformas.Runner>()
            .AddTransient<CancelProforma.Handler>()
            .AddTransient<SearchProformasNotAddedToInvoice.Runner>()
            .AddTransient<SearchProformasNotAddedToCollaboratorPayment.Runner>();

        return services;
    }
}