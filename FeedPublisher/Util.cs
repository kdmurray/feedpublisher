using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Text;
using System.Xml;

using EpubSharp;

namespace FeedPublisher
{
    class Util
    {
        public static string GenerateStartupMessage()
        {
            StringBuilder startMsg = new StringBuilder();
            startMsg.Append("Feed Publisher v");
            startMsg.AppendLine(GetVersionNumber());
            startMsg.AppendLine("(C) 2010 Keith Murray");
            startMsg.AppendLine("Released under an Apache 2.0 License");
            startMsg.AppendLine("");
            return startMsg.ToString();
        }

        public static string GetVersionNumber()
        {
            StringBuilder ver = new StringBuilder();
            ver.Append(Assembly.GetExecutingAssembly().GetName().Version.Major.ToString());
            ver.Append(".");
            ver.Append(Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString());
            ver.Append(".");
            ver.Append(Assembly.GetExecutingAssembly().GetName().Version.Build.ToString());
            return ver.ToString();
        }

        public static Metadata LoadFeedMetadata(SyndicationFeed feed)
        {
            Metadata md = new Metadata();
            md.Title = feed.Title.Text;
            md.Language = feed.Language;
            md.Identifier = feed.Id;
            string Creators = "";

            //TODO: There's probably a better way for EpubSharp to handle creators
            //HACK: 9-Jan-2011: This shit is ugly, needs to be refactored into its own method. There's too many ways for feeds to display authors
            foreach (SyndicationPerson creator in feed.Authors)
            {
                if (null != creator.Name)
                {
                    Creators += creator.Name.ToString() + ", ";
                }
                else
                {
                    if (null != creator.Email)
                    {
                        Creators += creator.Email.ToString() + ", ";
                    }
                }
            }
            if (Creators == "")
            {
                foreach (SyndicationItem article in feed.Items)
                {
                    foreach (SyndicationPerson creator in article.Authors)
                    {
                        if (null != creator.Name)
                        {
                            if (!Creators.Contains(creator.Name))
                            {
                                Creators += creator.Name.ToString() + ", ";
                            }
                        }
                        else
                        {
                            if (null != creator.Email)
                            {
                                if (!Creators.Contains(creator.Email))
                                {
                                    Creators += creator.Email.ToString() + ", ";
                                }
                            }
                        }
                    }
                }
            }

            if (Creators == "")
            {
                foreach (SyndicationItem article in feed.Items)
                {
                    foreach (string Name in (article.ElementExtensions.ReadElementExtensions<string>("creator", "http://purl.org/dc/elements/1.1/")))
                    {
                        if (!Creators.Contains(Name))
                        {
                            Creators += Name.ToString() + ", ";
                        }
                    }
                }
            }

            Creators = Creators.TrimEnd(", ".ToCharArray());
            if (Creators.Contains(", "))
            {
                int lastComma = Creators.LastIndexOf(", ");
                Creators = Creators.Substring(0, lastComma) + " &amp; " + Creators.Substring(lastComma + 2);
            }

            md.Creator = Creators;
            md.Rights = null == feed.Copyright ? "" : feed.Copyright.Text;
            md.Source = null == feed.BaseUri ? "" : feed.BaseUri.ToString();
            return md;
        }


        public static List<ContentComponent> GetContentsFromFeed(SyndicationFeed feed)
        {
            //TODO: The code to extract the content is a bit ugly due to the handling of extensions.

            List<ContentComponent> components = new List<ContentComponent>();

            foreach (SyndicationItem item in feed.Items)
            {
                ContentComponent cc = new ContentComponent();
                TextSyndicationContent content;

                cc.Title = item.Title.Text;

                if (null != item.Content)
                {
                    content = (TextSyndicationContent)item.Content;
                }
                else
                {
                    string contentString = "";

                    foreach (string encodedContent in (item.ElementExtensions.ReadElementExtensions<string>("encoded", "http://purl.org/rss/1.0/modules/content/")))
                    {
                        contentString = encodedContent;
                    }

                    if (contentString != "")
                    {
                        content = new TextSyndicationContent(contentString);
                    }
                    else
                    {
                        content = (TextSyndicationContent)item.Summary;
                    }
                }

                cc.Content = "<h2>" + item.Title.Text + "</h2>";
                cc.Content += content.Text;
                cc.MediaMimeType = "application/xhtml+xml";
                cc.ItemId = "part-" + components.Count.ToString();
                cc.UriString = cc.ItemId + ".html";

                components.Add(cc);
            }

            return components;
        }

        public static string MakeSafeFilename(string Filename)
        {
            char[] unsafeChars = System.IO.Path.GetInvalidFileNameChars();

            string goodFileName = Filename;

            foreach (char c in unsafeChars)
            {
                goodFileName = goodFileName.Replace(c.ToString(), "_");
            }

            return goodFileName;
        }
    }
}
