namespace WebAPI.BankBalance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankBalance(this IServiceCollection services)
    {
        services
            .AddTransient<GetBankBalance.Runner>()
            .AddTransient<ListBankBalance.Runner>();

        return services;
    }
}