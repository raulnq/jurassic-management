namespace WebAPI.ProformaDocuments
{
    public class ProformaDocument
    {
        public Guid ProformaId { get; private set; }
        public string Url { get; private set; } = default!;

        private ProformaDocument()
        {

        }

        public ProformaDocument(Guid proformaId, string url)
        {
            ProformaId = proformaId;
            Url = url;
        }
    }
}
