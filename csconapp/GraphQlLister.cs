namespace csconapp
{
    using System;
    using System.Net.Http;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    using Contentstack.Core;

    public class GraphQlLister
    {
    //    private int ContentstackQueryResultLimit { get; } = 100;

        public void Go(ContentstackClient stack, OutputWriter writer)
        {
            // https://www.contentstack.com/docs/developers/apis/content-delivery-api/#get-all-content-types
            foreach (JObject contentTypeJson in stack.GetContentTypes().GetAwaiter().GetResult())
            {
                string contentTypeUid = contentTypeJson["uid"].ToString();

                if (contentTypeJson.SelectToken("$..[?(@.uid=='url')]") == null)
                {
                    writer.Message(this, contentTypeUid + " does not define URL field; skip.");
                    continue;
                }

                new Thread(() => { ProcessContentType(stack, contentTypeUid, writer); }).Start();
            }
        }

        private void ProcessContentType(ContentstackClient stack, string contentTypeUid, OutputWriter writer)
        {
            //TODO: paging results with threads?
            // https://www.contentstack.com/docs/developers/apis/graphql-content-delivery-api/explorer/?api_key=APIKEY&access_token=DELIVERY_TOKEN&environment=ENVIRONMENT
            string query = "{all_" + contentTypeUid + "{total items{url title system{uid}}}}";

            using (HttpClient http = new HttpClient())
            {
                string request = "https://graphql.contentstack.com/stacks/" +
                    stack.GetApplicationKey() + "?access_token=" +
                    stack.GetAccessToken() + "&environment=" +
                    stack.GetEnvironment() + "contentdelivery&query=" + query;
                int attempt = 0;

                do
                {
                    try
                    {
                        JObject fromGraphQl = JObject.Parse(http.GetStringAsync(request).GetAwaiter().GetResult());
                        int count = Int32.Parse(fromGraphQl["data"]["all_" + contentTypeUid]["total"].ToString());

                        foreach (JToken group in fromGraphQl.SelectTokens("$.data.all_" + contentTypeUid + ".items"))
                        {
                            JToken myGroup = group;

                            foreach (var entry in myGroup.Children())
                            {
                                string entryUid = entry.SelectToken("system").SelectToken("uid").ToString();
                                writer.Message(this, $"{entryUid} [{contentTypeUid}] : {entry["title"]} ({entry["url"]})");
                            }
                        }

                        attempt = 3;
                    }
                    catch (Exception ex)
                    {
                        writer.Message(this, ex + " : " + ex.Message);
                    }
                }
                while (attempt++ < 3);
            }
        }
    }
}