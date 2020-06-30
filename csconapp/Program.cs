// Contentstack Console App
namespace csconapp
{
    // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/
    using Contentstack.Core;
    using Contentstack.Core.Models;
    using Contentstack.Core.Internals;

    public class Program
    {
        //TODO: replace with values from your Stack
        private static readonly string APIKEY = "blt94519f01d8f92c86";
        private static readonly string DELIVERY_TOKEN = "cs99af6674ef2a7a53770da6b7";
        private static readonly string ENVIRONMENT = "contentdelivery";

        private static readonly string CONTENT_TYPE_UID = "resource";
        private static readonly string ENTRY_UID = "blt9bfc410b2e10a6f0";
        private static readonly string ENTRY_URL = "/";

        private static readonly string ASSET_ID = "blt6810b5d167a24132";

        static void Main(string[] args)
        {
            https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.ContentstackClient.html
            ContentstackClient stack = new ContentstackClient(
                APIKEY,
                DELIVERY_TOKEN,
                ENVIRONMENT);
            OutputWriter writer = new OutputWriter();
            new KnownEntry().Go(stack, CONTENT_TYPE_UID, ENTRY_UID, writer);
            new KnownUrl().Go(stack, ENTRY_URL, writer);
            new FieldSubsetQueryExample().Go(stack, writer);
            new GraphQlLister().Go(stack, writer);
            new SyncEventLister().Go(stack, writer);
            new EntryModelExample().Go(stack, CONTENT_TYPE_UID, ENTRY_UID, writer);
            new AssetExample().Go(stack, ASSET_ID, writer);
        }
    }
}

