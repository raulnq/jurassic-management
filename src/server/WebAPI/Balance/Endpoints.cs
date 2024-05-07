namespace WebAPI.Balance;

public static class Endpoints
{
    public const string Title = "Balance";

    public const string ListTitle = "List balance";

    public const string List = "/ui/balance/list";

    public static void RegisterBalanceEndpoints(this WebApplication app)
    {
        var uigroup = app.MapGroup("/ui/balance")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListBalance.HandlePage);
    }
}
