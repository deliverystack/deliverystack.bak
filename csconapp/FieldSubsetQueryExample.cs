namespace csconapp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    using Contentstack.Core;
    using Contentstack.Core.Models;

    /// <summary>
    /// Examples of accessing a subset of the Fields in Contentstack Entries.
    /// </summary>
    class FieldSubsetQueryExample
    {
        private int ContentstackQueryResultLimit { get; } = 100;

        private class FieldSubsetEntry
        {
            public string Url { get; set; }
            public string Title { get; set; }
        }

        public void Go(
            ContentstackClient stack,
            OutputWriter writer)
        {
            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-all-content-types
            foreach (JObject contentTypeJson in stack.GetContentTypes().GetAwaiter().GetResult())
            {
                ContentType contentType = stack.ContentType(contentTypeJson["uid"].ToString());
                new Thread(() => { ProcessContentType(contentType, writer); }).Start();
            }
        }

        private void ProcessContentType(ContentType contentType, OutputWriter writer)
        {
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.Query.html
            Query query = contentType.Query().Only(new[] { "title", "url" }).Limit(ContentstackQueryResultLimit);
            query.IncludeCount();

            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#queries
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.ContentstackCollection-1.html
            ContentstackCollection<FieldSubsetEntry> firstBatch = query.Find<FieldSubsetEntry>().GetAwaiter().GetResult();

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
                new Thread(() => { QueryBatch((skip - ContentstackQueryResultLimit) / ContentstackQueryResultLimit, contentType, mySkip, writer); }).Start();
            }
        }

        private void ProcessBatch(int batchNumber, ContentstackCollection<FieldSubsetEntry> entries, string contentTypeUid, OutputWriter writer)
        {
            foreach (FieldSubsetEntry entry in entries.Items)
            {
                writer.Message(this, $"batch {batchNumber} [{contentTypeUid}] : {entry.Title}");
            }
        }

        private void QueryBatch(int batchNumber, ContentType contentType, int skip, OutputWriter writer)
        {
            ProcessBatch(batchNumber, contentType.Query().Only(new[] { "url", "title" }).Limit(ContentstackQueryResultLimit)
                .Skip(skip).Find<FieldSubsetEntry>().GetAwaiter().GetResult(), contentType.ContentTypeId, writer);
        }
    }
}

