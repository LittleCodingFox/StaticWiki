using System;
using System.Linq;

namespace StaticWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDirectory = "";
            string destinationDirectory = "";
            string themeFileName = "";
            string titleName = "";
            string navigationFileName = "";
            string[] contentExtensions = new string[0];
            bool disableAutoPageExtension = false;
            bool disableLinkCorrection = false;

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
                else if (args[i] == "-theme" && i + 1 < args.Length)
                {
                    themeFileName = args[i + 1];
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
                if(!StaticWikiCore.GetWorkspaceDetails(workspaceDirectory, ref sourceDirectory, ref destinationDirectory, ref themeFileName, ref titleName, ref navigationFileName, ref contentExtensions,
                    ref disableAutoPageExtension, ref disableLinkCorrection, ref logMessage))
                {
                    Console.WriteLine(logMessage);

                    return;
                }
            }

            Console.WriteLine("StaticWiki starting up with values:");
            Console.WriteLine(string.Format("From Directory: \"{0}\"", sourceDirectory));
            Console.WriteLine(string.Format("To Directory: \"{0}\"", destinationDirectory));
            Console.WriteLine(string.Format("Theme File: \"{0}\"", themeFileName));
            Console.WriteLine(string.Format("Navigation File: \"{0}\"", navigationFileName));
            Console.WriteLine(string.Format("Content Extensions: \"{0}\"", string.Join(", ", contentExtensions.Select(x => string.Format(".{0}", x.Trim())).ToArray())));
            Console.WriteLine(string.Format("Base Page Title: \"{0}\"", titleName));
            Console.WriteLine(string.Format("Auto Page Extensions are {0}", disableAutoPageExtension ? "DISABLED" : "ENABLED"));
            Console.WriteLine(string.Format("Link Correction is {0}", disableLinkCorrection ? "DISABLED" : "ENABLED"));

            StaticWikiCore.ProcessDirectory(sourceDirectory, destinationDirectory, themeFileName, navigationFileName, contentExtensions.ToArray(), titleName, disableAutoPageExtension,
                disableLinkCorrection, ref logMessage);

            Console.WriteLine(logMessage);
        }
    }
}
