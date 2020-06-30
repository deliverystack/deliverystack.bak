namespace csconapp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    using Contentstack.Core;
    using Contentstack.Core.Models;

    public class KnownUrl
    {
        private int ContentstackQueryResultLimit { get; } = 100;

        public void Go(
            ContentstackClient stack,
            string url,
            OutputWriter writer)
        {
            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-all-content-types
            foreach (JObject contentTypeJson in stack.GetContentTypes().GetAwaiter().GetResult())
            {
                ContentType contentType = stack.ContentType(contentTypeJson["uid"].ToString());
                new Thread(() => { ProcessContentType(contentType, url, writer); }).Start();
            }
        }

        private void ProcessContentType(ContentType contentType, string url, OutputWriter writer)
        {
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.Query.html
            Query query = contentType.Query().Where("url", url).Limit(ContentstackQueryResultLimit);
            query.IncludeCount();

            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#queries
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.ContentstackCollection-1.html
            ContentstackCollection<Entry> firstBatch = query.Find<Entry>().GetAwaiter().GetResult();

            if (firstBatch.Count < 1)
            {
                return;
            }

            new Thread(() => { ProcessBatch(1, firstBatch, contentType.ContentTypeId, writer); }).Start();

            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#pagination
            for (int skip = ContentstackQueryResultLimit; skip < firstBatch.Count; skip += ContentstackQueryResultLimit)
            {
                //WARN: https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                int mySkip = skip;
                new Thread(() => { QueryBatch((mySkip - ContentstackQueryResultLimit) / ContentstackQueryResultLimit, contentType, url, mySkip, writer); }).Start();
            }
        }

        private void ProcessBatch(int batchNumber, ContentstackCollection<Entry> entries, string contentTypeUid, OutputWriter writer)
        {
            foreach (Entry entry in entries.Items)
            {
                string message = $"batch {batchNumber} [{contentTypeUid}] : {entry.Uid} : {entry.Title}";

                if (entry.Object.ContainsKey("url"))
                {
                    message += " (" + entry.Object["url"] + ")";
                }

                writer.Message(this, message);
            }
        }

        private void QueryBatch(int batchNumber, ContentType contentType, string url, int skip, OutputWriter writer)
        {
            ProcessBatch(batchNumber, contentType.Query().Where("url", url).Limit(ContentstackQueryResultLimit)
                .Skip(skip).Find<Entry>().GetAwaiter().GetResult(), contentType.ContentTypeId, writer);
        }
    }
}