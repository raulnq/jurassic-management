using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure
{
    public class AzureBlobStorage
    {
        protected readonly BlobContainerClient _container;

        protected AzureBlobStorage(string connectionString, string container)
        {
            _container = new BlobContainerClient(connectionString, container);

            _container.CreateIfNotExistsAsync();
        }

        protected async Task<string> Upload(string name, Stream stream, string contentType)
        {
            var blob = _container.GetBlobClient(name);

            var headers = new BlobHttpHeaders()
            {
                ContentType = contentType,
            };

            await blob.UploadAsync(stream, headers);

            return blob.Uri.AbsoluteUri;
        }
    }
}
