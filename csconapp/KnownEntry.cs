namespace csconapp
{
    using System;

    using Contentstack.Core;
    using Contentstack.Core.Models;
    using Newtonsoft.Json.Linq;

    // retrieve a single Entry
    // possibly most efficient when Content Type and UID are known.
    public class KnownEntry
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
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.Entry.html
            contentType.Entry(entryUid).Fetch<Entry>().ContinueWith((t) =>
            {
                Entry entry = t.Result;
                string message = $"Asynchronous: {entry.Uid} : {entry.Title}";
                JObject entryJson = entry.ToJson();

                if (entryJson.ContainsKey("url"))
                {
                    message += " (" + entryJson["url"] + ")";
                }

                writer.Message(this, message);
            });

            Entry entry = contentType.Entry(entryUid).Fetch<Entry>().GetAwaiter().GetResult();

            if (entry != null)
            {
                string message = $"Synchronous: {entry.Uid} : {entry.Title}";
                JObject entryJson = entry.ToJson();

                if (entryJson.ContainsKey("url"))
                {
                    message += " (" + entryJson["url"] + ")";
                }

                writer.Message(this, message);
            }
        }
    }
}
