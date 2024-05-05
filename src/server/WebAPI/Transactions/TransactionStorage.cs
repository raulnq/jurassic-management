using Infrastructure;
using System.Net.Mime;

namespace Transactions
{
    public class TransactionStorage : AzureBlobStorage
    {
        public TransactionStorage(string connectionString) : base(connectionString, "transactions")
        {
        }

        public Task<string> Upload(string name, Stream stream)
        {
            return Upload(name, stream, MediaTypeNames.Application.Pdf);
        }
    }
}
