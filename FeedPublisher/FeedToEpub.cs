using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;

using EpubSharp;

namespace FeedPublisher
{
    class FeedToEpub
    {
        public static void CreateEpubDocument(string FeedUrl)
        {
            List<ContentComponent> contentPages = new List<ContentComponent>();
            List<ContentComponent> contentImages = new List<ContentComponent>();
            Metadata md = new Metadata();

            Console.WriteLine("Downloading feed...");

            XmlReader xr = null;
            SyndicationFeed feed = null;

            try
            {

                xr = XmlReader.Create(FeedUrl);
                feed = SyndicationFeed.Load(xr);
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Message.Contains("404"))
                {
                    Console.WriteLine("ERROR: feed not found.");
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            contentPages = Util.GetContentsFromFeed(feed);

            foreach (ContentComponent page in contentPages)
            {
                if (page.MediaMimeType == "application/xhtml+xml")
                {
                    try
                    {
                        Console.WriteLine("Downloading images for {0}", page.Title);
                        List<string> images = EpubSharp.Util.ParseImageTags(page.Content);
                        foreach (string image in images)
                        {
                            ImageContentComponent ic = new ImageContentComponent(image);
                            page.Content = page.Content.Replace(ic.OriginalUri, ic.PartUri.ToString().TrimStart("/".ToCharArray()));
                            contentImages.Add(ic);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("404"))
                        {
                            Console.WriteLine("ERROR: image not found.");
                        }
                        else
                        {
                            Console.WriteLine("ERROR: " + ex.Message);
                        }
                    }
                }
            }

            foreach (ContentComponent cc in contentImages)
            {
                contentPages.Add(cc);
            }

            Console.WriteLine("Transforming feed metadata...");
            md = Util.LoadFeedMetadata(feed);

            Console.WriteLine("Writing EPUB document...");
            Document d = new Document();

            d.CreateDocument(Util.MakeSafeFilename(md.Title.ToString()) + ".epub", md, contentPages);

            Console.WriteLine("DONE!");
        }
    }
}
