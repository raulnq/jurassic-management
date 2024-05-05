namespace WebAPI.Collections;

public static class Endpoints
{
    public const string List = "/ui/collections/list";

    public const string View = "/ui/collections/{collectionId}/view";

    public const string Confirm = "/ui/collections/{collectionId}/confirm";

    public const string Cancel = "/ui/collections/{collectionId}/cancel";

    public static string GetView(Guid collectionId)
    {
        return View.Replace("{collectionId}", collectionId.ToString());
    }
    public static string GetConfirm(Guid collectionId)
    {
        return Confirm.Replace("{collectionId}", collectionId.ToString());
    }

    public static string GetCancel(Guid collectionId)
    {
        return Cancel.Replace("{collectionId}", collectionId.ToString());
    }


    public static void RegisterCollectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collections")
        .WithTags("collections");

        group.MapPost("/{collectionId:guid}/confirm", ConfirmCollection.Handle);

        group.MapGet("/", ListCollections.Handle);

        group.MapGet("/{collectionId:guid}", GetCollection.Handle);

        var uigroup = app.MapGroup("/ui/collections")
           .ExcludeFromDescription();

        uigroup.MapGet("/list", ListCollections.HandlePage);

        uigroup.MapGet("/{collectionId:guid}/view", GetCollection.HandlePage);

        uigroup.MapGet("/{collectionId:guid}/confirm", ConfirmCollection.HandlePage);

        uigroup.MapPost("/{collectionId:guid}/confirm", ConfirmCollection.HandleAction);

        uigroup.MapPost("/{collectionId:guid}/cancel", CancelCollection.HandleAction);
    }
}