namespace WebAPI.Collections;

public static class Endpoints
{
    public static void RegisterCollectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/collections")
        .WithTags("collections");

        group.MapPost("/{collectionId:guid}/confirm", ConfirmCollection.Handle);

        group.MapGet("/", ListCollections.Handle);

        group.MapGet("/{collectionId:guid}", GetCollection.Handle);
    }
}