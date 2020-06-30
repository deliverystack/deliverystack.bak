namespace csconapp
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Contentstack.Core;
    using Contentstack.Core.Models;

    /// <summary>
    /// Examples of accessing a Contentstack Entry using an Entry Model class.
    /// </summary>
    public class EntryModelExample
    {
        public void Go(
            ContentstackClient stack,
            string contentTypeUid,
            string entryUid,
            OutputWriter writer)
        {
            //TODO: does this really invoke REST API?
            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-a-single-content-type
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.ContentType.html
            ContentType contentType = stack.ContentType(contentTypeUid);

            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-a-single-entry
            BasicEntry directEntry = contentType.Entry(entryUid).Fetch<BasicEntry>().GetAwaiter().GetResult();
            writer.Message(this, "Direct: " + directEntry.CreatedAt);
            Entry entry = contentType.Entry(entryUid).Fetch<Entry>().GetAwaiter().GetResult();
            JObject entryJson = entry.ToJson();
            BasicEntry convertedEntry = entryJson.ToObject<BasicEntry>(JsonSerializer.Create(stack.SerializerSettings));
            writer.Message(this, "Converted: " + directEntry.CreatedAt);
        }
    }
}
