using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StaticWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("StaticWiki started with no options. The options available are:");
                Console.WriteLine("StaticWiki -from FromDirectory -to ToDirectory -theme themefile -title title");
                Console.WriteLine();
                Console.WriteLine("From Directory should contain multiple .txt files that contain Markdown code");
                Console.WriteLine("Theme file should specify a text file file with special section keywords to replace with page contents.");
                Console.WriteLine("Processed pages will have the same extension as the theme file.");
                Console.WriteLine("Title is the base page title - It will become be \"Title - Current Page Title\"");
                Console.WriteLine("Current Page Title will have \"_\"'s removed");
                Console.WriteLine("Special sections are:");
                Console.WriteLine("\t\t{TITLE} - should be placed on the <title> tag");
                Console.WriteLine("\t\t{CONTENT} - should be placed where you want the page content to show");
                Console.WriteLine("\t\t{SEARCHNAMES} - A list of javascript strings containing the page names");
                Console.WriteLine("\t\t{SEARCHADDRESSES} - A list of javascript strings containing the page addresses");
                Console.WriteLine("\t\t{BEGINNAV} - Begins a code snippet for navigation");
                Console.WriteLine("\t\t{ENDNAV} - Ends a code snippet for navigation");
                Console.WriteLine("\t\t{NAVNAME} - The name of the navigation item");
                Console.WriteLine("\t\t{NAVLINK} - The link of the navigation item");
            }

            var fromDirectory = ".";
            var toDirectory = "./Out/";
            var themeFileName = "";
            var basePageTitle = "TEMPLATE";
            var navigationFileName = "Navigation.list";

            var fileCache = new Dictionary<string, FileInfo>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-from" && i + 1 < args.Length)
                {
                    fromDirectory = args[i + 1];
                }
                else if (args[i] == "-to" && i + 1 < args.Length)
                {
                    toDirectory = args[i + 1];
                }
                else if (args[i] == "-theme" && i + 1 < args.Length)
                {
                    themeFileName = args[i + 1];
                }
                else if (args[i] == "-title" && i + 1 < args.Length)
                {
                    basePageTitle = args[i + 1];
                }
                else if(args[i] == "-navigation" && i + 1 < args.Length)
                {
                    navigationFileName = args[i + 1];
                }
            }

            Console.WriteLine("StaticWiki starting up with values:");
            Console.WriteLine(string.Format("From Directory: \"{0}\"", fromDirectory));
            Console.WriteLine(string.Format("To Directory: \"{0}\"", toDirectory));
            Console.WriteLine(string.Format("Theme File: \"{0}\"", themeFileName));
            Console.WriteLine(string.Format("Base Page Title: \"{0}\"", basePageTitle));

            string logMessage = "";

            StaticWikiCore.ProcessDirectory(fromDirectory, toDirectory, themeFileName, navigationFileName, basePageTitle, ref logMessage);

            Console.WriteLine(logMessage);
        }
    }
}
