using Infrastructure;
using System.Net.Mime;

namespace Transactions
{
    public class TransactionStorage : AzureBlobStorage
    {
        public TransactionStorage(string connectionString) : base(connectionString, "transactions")
        {
        }
    }
}
