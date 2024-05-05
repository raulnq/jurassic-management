using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Users;

public static class Endpoints
{
    public const string Login = "/ui/login";

    public static void RegisterUserEndpoints(this WebApplication app)
    {
        var uigroup = app.MapGroup("/ui")
        .ExcludeFromDescription();

        uigroup.MapGet("/login", LoginUser.HandlePage);

        uigroup.MapPost("/login", LoginUser.HandleAction);

        uigroup.MapGet("/", () =>
        {
            return new RazorComponentResult<MainPage>();
        });

    }
}
