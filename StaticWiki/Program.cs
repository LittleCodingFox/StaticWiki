using System;
using System.Collections.Generic;
using System.Linq;

namespace StaticWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDirectory = "";
            string destinationDirectory = "";
            string defaultThemeName = "";
            KeyValuePair<string, string>[] themes = new KeyValuePair<string, string>[0];
            string titleName = "";
            string navigationFileName = "";
            string[] contentExtensions = new string[0];
            bool disableAutoPageExtension = false;
            bool disableLinkCorrection = false;
            bool showCategoryPrefixInCategoryPageTitles = true;
            string[] markdownExtensions = new string[0];

            if (args.Length == 0)
            {
                Console.WriteLine("StaticWiki started with no options. The options available are:");
                Console.WriteLine("StaticWiki -from FromDirectory -to ToDirectory -theme themefile -navigation navigationfile -content contentextension1 -content contentextension2 -title title");
                Console.WriteLine("Alternative: StaticWiki -workspace WorkspaceDirectory");
                Console.WriteLine();
                Console.WriteLine("See the README file for details on how to use Static Wiki");

                return;
            }

            var workspaceDirectory = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-from" && i + 1 < args.Length)
                {
                    sourceDirectory = args[i + 1];
                }
                else if (args[i] == "-to" && i + 1 < args.Length)
                {
                    destinationDirectory = args[i + 1];
                }
                else if (args[i] == "-title" && i + 1 < args.Length)
                {
                    titleName = args[i + 1];
                }
                else if(args[i] == "-navigation" && i + 1 < args.Length)
                {
                    navigationFileName = args[i + 1];
                }
                else if(args[i] == "-content" && i + 1 < args.Length)
                {
                    contentExtensions = contentExtensions.Concat(new string[] { args[i + 1] }).ToArray();
                }
                else if(args[i] == "-workspace" && i + 1 < args.Length)
                {
                    workspaceDirectory = args[i + 1];
                }
            }

            string logMessage = "";

            if (workspaceDirectory.Length > 0)
            {
                if(!StaticWikiCore.GetWorkspaceDetails(workspaceDirectory, ref sourceDirectory, ref destinationDirectory, ref defaultThemeName,
                    ref themes, ref titleName, ref navigationFileName, ref contentExtensions, ref disableAutoPageExtension,
                    ref disableLinkCorrection, ref markdownExtensions, ref showCategoryPrefixInCategoryPageTitles, ref logMessage))
                {
                    Console.WriteLine(logMessage);

                    return;
                }
            }

            Console.WriteLine("StaticWiki starting up with values:");
            Console.WriteLine(string.Format("From Directory: \"{0}\"", sourceDirectory));
            Console.WriteLine(string.Format("To Directory: \"{0}\"", destinationDirectory));
            Console.WriteLine(string.Format("Theme File: \"{0}\"", defaultThemeName));
            Console.WriteLine(string.Format("Navigation File: \"{0}\"", navigationFileName));
            Console.WriteLine(string.Format("Content Extensions: \"{0}\"", string.Join(", ", contentExtensions.Select(x => string.Format(".{0}", x.Trim())).ToArray())));
            Console.WriteLine(string.Format("Base Page Title: \"{0}\"", titleName));
            Console.WriteLine(string.Format("Auto Page Extensions are {0}", disableAutoPageExtension ? "DISABLED" : "ENABLED"));
            Console.WriteLine(string.Format("Link Correction is {0}", disableLinkCorrection ? "DISABLED" : "ENABLED"));
            Console.WriteLine(string.Format("Category Prefix in Category Page Titles is {0}",
                showCategoryPrefixInCategoryPageTitles ? "ENABLED" : "DISABLED"));
            Console.WriteLine(string.Format("Markdown Extensions: \"{0}\"", string.Join(", ", markdownExtensions)));

            StaticWikiCore.ProcessDirectory(sourceDirectory, destinationDirectory, defaultThemeName, themes, navigationFileName,
                contentExtensions.ToArray(), titleName, disableAutoPageExtension, disableLinkCorrection, markdownExtensions,
                showCategoryPrefixInCategoryPageTitles, ref logMessage);

            Console.WriteLine(logMessage);
        }
    }
}
