namespace csconapp
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class can represent any Entry and can serve as a base class for Entry Models
    /// (classes that represent individual Content Types).
    /// </summary>
    public class BasicEntry
    {
        // for efficiency, only expose the fields that you need
        // see https://www.contentstack.com/docs/developers/dot-net/contentstack-net-model-generator/

        //TODO: set explicitly after instantiation
        public string ContentTypeUid { get; set; }
        public JObject JObject { get; set; }

        public string Title { get; set; }

        [JsonProperty(propertyName: "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(propertyName: "uid")]
        public string EntryUid { get; set; }

        public string Locale { get; set; }

        public string[] Tags { get; set; }

        [JsonProperty(propertyName: "_version")]
        public Double Version { get; set; }

        [JsonProperty(propertyName: "created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(propertyName: "created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty(propertyName: "updated_by")]
        public string UpdatedBy { get; set; }

        [JsonProperty(propertyName: "publish_details")]
        public PublishDetails PublishDetails { get; set; }

        [JsonProperty(propertyName: "deleted_at")]
        public DateTime DeletedAt { get; set; } = DateTime.MaxValue;

        [JsonProperty(propertyName: "deleted_by")]
        public string DeletedBy { get; set; }

        // always null for Content Block Content Types
        public string Url { get; set; }
    }
}
