namespace csconapp
{
    using Contentstack.Core;
    using Contentstack.Core.Models;

    /// <summary>
    /// Example of accessing a Contentstack Asset.
    /// </summary>
    public class AssetExample
    {
        public void Go(
            ContentstackClient stack,
            string assetUid,
            OutputWriter writer)
        {
            Asset asset = stack.Asset(assetUid).Fetch().Result;
            writer.Message(this, asset.Url + " : " + asset.FileSize);
        }
    }
}