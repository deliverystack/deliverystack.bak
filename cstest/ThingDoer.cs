
namespace CommandName
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using Contentstack.Core;
    using Deliverystack.Core.Models.Entries;
    using Deliverystack.Core.Models.Repositories;
    using Root.Core.Models.Entries.Author;
    using Root.Core.Models.Entries.Resource;
    using Root.Core.Models.Fields;
    using Root.Core.Models.GlobalFields.Elements;

    public class ThingDoer
    {
        private void McHammer()
        {
            double i = 0;
            int limit = 1000;
            DateTime start = DateTime.Now;

            using (HttpClient httpClient = new HttpClient())
            {
                do
                {
                    try
                    {
                        if (i % 100 == 0)
                        {
                            Console.Write("#");
                        }

                        string discard = httpClient.GetStringAsync("https://localhost:5001/").Result;
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex.GetType() + " : " + ex.Message);
                        // break on first error
                        limit = 0;
                    }
                } while (i++ < limit);
            }

            TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
            double ms = timeSpan.Ticks / 10000;
            double msPerRequest = ms / --i;
            double requestPerSec = i / ms;
            Console.WriteLine($"{this} : {i} requests in {ms}ms ({Math.Round(msPerRequest * 1000) / 1000} ~ms/request; ~{Math.Round(requestPerSec * 1000)} request/sec)");
        }

        public void DoThing(ContentstackClient client, ContentstackRepository repository)
        {
            if (true)
            {
                McHammer();
            }
            
            ResourceEntry entry = repository.Get<ResourceEntry>("blt9bfc410b2e10a6f0");

            foreach (var thing in repository.GetChildren(entry.EntryUid, entry.ContentTypeUid))
            {
                Console.WriteLine("CHILD: " + thing);
            }



            Console.WriteLine(repository.GetChildIdentifiers(entry.EntryUid, entry.ContentTypeUid));

            foreach (Tuple<string, string> childIds in repository.GetChildIdentifiers(entry.EntryUid,
                entry.ContentTypeUid))
            {
                EntryPointer child = repository.Get<EntryPointer>(childIds.Item1, childIds.Item2);
                Console.WriteLine("Child: " + child.Url);
            }
            
            foreach (var block in entry.Elements.Members)
            {
                switch (block.ElementType)
                {
                    case ElementType.CodeBlock:
                        Console.WriteLine("CodeBlock: " + ((CodeBlockElement) block).Code);
                        break;
                    case ElementType.CodeLink:
                        Console.WriteLine("CodeLink: " + ((CodeLinkElement) block).Url);
                        break;
                    case ElementType.Download:
                        Console.WriteLine("Download: " + ((DownloadElement) block).Asset.Value.Url);
                        break;
                    case ElementType.Image:
                        Console.WriteLine("Image: " + ((ImageElement) block).Asset.Value.Url);
                        break;
                    case ElementType.Markdown:
                        Console.WriteLine("Markdown: " + ((MarkdownElement) block).Markdown.Markup);
                        break;
                    case ElementType.RichText:
                        Console.WriteLine("RichText: " + ((MarkupElement) block).RichText.Value);
                        break;
                    default:
                        Trace.Assert(false, 
                        String.Format("{0} {1} not recognized", block.ElementType.GetType(), block.ElementType));
                        break;
                }
            }

            foreach (ReferenceField<AuthorEntry> field in entry.PageData.Authors)
            {
                Console.WriteLine("Author: "+ field.Get().Name);
            }
        }
    }
}