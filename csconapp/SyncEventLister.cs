namespace csconapp
{
    using System;
    using System.Linq;

    using Newtonsoft.Json.Linq;

    using Contentstack.Core;
    using Contentstack.Core.Internals;
    using Contentstack.Core.Models;
    using Newtonsoft.Json;

    public class SyncEventLister
    {
        public void Go(ContentstackClient stack, OutputWriter writer)
        {
            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#initial-sync
            // https://www.contentstack.com/docs/platforms/dot-net/api-reference/api/Contentstack.Core.Models.SyncStack.html
            SyncStack syncStack = stack.SyncRecursive(
                null,           /*Locale*/
                SyncType.All,
                null,           /* ContentTypeUID */
                DateTime.MinValue).Result;

            //WARN: in case of multiple events for an Entry, threads running out of event date sequence could load stale data
            foreach (JObject entryEvent in syncStack.items)
            {
                //WARN: https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                JObject myEntryEvent = entryEvent;
                ProcessMessage(stack, myEntryEvent, writer);
            }

            string syncToken = syncStack.sync_token;

            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#subsequent-sync
            SyncStack updatesSinceSync = stack.SyncToken(syncToken).Result;
            writer.Message(this, updatesSinceSync.items.Count() + " update(s) since last sync");

            foreach (JObject entryEvent in updatesSinceSync.items)
            {
                //WARN: https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp
                JObject myEntryEvent = entryEvent;
                ProcessMessage(stack, myEntryEvent, writer);
            }
        }

        private void ProcessMessage(ContentstackClient stack, JObject entryEvent, OutputWriter writer)
        {
            string contentTypeUid = entryEvent["content_type_uid"].ToString();
            string eventType = entryEvent["type"].ToString();
            DateTime eventDate = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.Parse(entryEvent["event_at"].ToString()), TimeZoneInfo.Local);
            string entryUid = null;

            if (entryEvent["data"].SelectToken("uid") != null)
            {
                entryUid = entryEvent["data"]["uid"].ToString();
            }

            switch (eventType)
            {
                case "entry_published":
                    // optional (retrieve metadata for media assets)
                    foreach (JValue toReplace in entryEvent.SelectTokens("$..asset"))
                    {
                        string assetId = toReplace.ToString();

                        // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-a-single-asset
                        Asset asset = stack.Asset(assetId).Fetch().GetAwaiter().GetResult();
                        JObject withJson = new JObject();
                        withJson["url"] = asset.Url;
                        withJson["filename"] = asset.FileName;
                        toReplace.Replace(withJson);
                    }

                    Entry entry = entryEvent["data"].ToObject<Entry>(JsonSerializer.Create(stack.SerializerSettings));
                    writer.Message(this, $"{entryUid} [{contentTypeUid}] : {entry.Title}");
                    break;
                case "asset_published":
                case "asset_unpublished":
                case "asset_deleted":
                case "content_type_deleted":
                case "entry_unpublished":
                case "entry_deleted":
                default:
                    writer.Message(this, $"{eventDate} : {eventType}");
                    break;
            }
        }
    }
}
