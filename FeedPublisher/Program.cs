using System;
using System.IO;


using EpubSharp;

namespace FeedPublisher
{
    class Program
    {
        static string _feedFile;

        static void Main(string[] args)
        {

            Console.WriteLine(Util.GenerateStartupMessage());

            DateTime Start = DateTime.Now;

            ParseParams(args);

            if (null == _feedFile)
            {
                _feedFile = "feeds.txt";
            }

            string[] Feeds = ReadFeedsFromFile(_feedFile);

            foreach (string feed in Feeds)
            {
                if (!feed.StartsWith("#"))
                {
                    FeedToEpub.CreateEpubDocument(feed);
                }
            }

            DateTime End = DateTime.Now;

            TimeSpan ts = End - Start;
            Console.WriteLine("Elapsed time: {0}", ts.ToString());

        }

        static void ParseParams(string[] args)
        {
            foreach (string param in args)
            {
                if (param.StartsWith("-f=") || param.StartsWith("--feedfile="))
                {
                    _feedFile = param.Substring(param.IndexOf("=") + 1);
                }
            }
        }

        static string[] ReadFeedsFromFile(string FeedFile)
        {
            if (File.Exists(FeedFile))
            {
                return File.ReadAllLines(FeedFile);
            }
            throw new FileNotFoundException(FeedFile + " cannot be found.");
        }
    }
}
