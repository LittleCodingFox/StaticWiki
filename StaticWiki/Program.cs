using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MarkdownSharp;

namespace StaticWiki
{
    class FileInfo
    {
        public String BaseName, Text;
    };

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("StaticWiki started with no options. The options available are:");
                Console.WriteLine("StaticWiki -from FromDirectory -to ToDirectory -theme themefolder -title pagetitle");
                Console.WriteLine();
                Console.WriteLine("From Directory should contain multiple .txt files that contain Markdown code");
                Console.WriteLine("Theme folder should contain a theme.html file with special section keywords to replace with page contents");
                Console.WriteLine("Special sections are:");
                Console.WriteLine("\t\t{TITLE} - should be placed on the <title> tag");
                Console.WriteLine("\t\t{CONTENT} - should be placed where you want the page content to show");
                Console.WriteLine("\t\t{CATEGORIES} - should be placed where you want the category listings to show");
                Console.WriteLine("\t\t{SEARCHNAMES} - A list of javascript strings containing the page names");
                Console.WriteLine("\t\t{SEARCHADDRESSES} - A list of javascript strings containing the page addresses");
                Console.WriteLine("Page Title is actually base page title - Page title will actually be \"PageTitle - CurrentPageTitle\"");
                Console.WriteLine("Current Page Title will have \"_\"'s removed");
            }

            String FromDirectory = ".", ToDirectory = "./Out/", ThemeFolder = "", BasePageTitle = "TEMPLATE";

            Dictionary<String, FileInfo> FileCache = new Dictionary<String, FileInfo>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-from" && i + 1 < args.Length)
                {
                    FromDirectory = args[i + 1];
                }
                else if (args[i] == "-to" && i + 1 < args.Length)
                {
                    ToDirectory = args[i + 1];
                }
                else if (args[i] == "-theme" && i + 1 < args.Length)
                {
                    ThemeFolder = args[i + 1];
                }
                else if (args[i] == "-title" && i + 1 < args.Length)
                {
                    BasePageTitle = args[i + 1];
                }
            }

            string[] files = Directory.GetFiles(FromDirectory, "*.txt");

            Markdown Processor = new Markdown();

            Console.WriteLine("StaticWiki starting up with values:");
            Console.WriteLine("From Directory: \"" + FromDirectory + "\"");
            Console.WriteLine("To Directory: \"" + ToDirectory + "\"");
            Console.WriteLine("Theme Folder: \"" + ThemeFolder + "\\theme.html\"");
            Console.WriteLine("Base Page Title: \"" + BasePageTitle + "\"");

            String ThemeText = "";

            try
            {
                StreamReader In = new StreamReader(ThemeFolder + "\\theme.html");

                ThemeText = In.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read theme file \"" + ThemeFolder + "\\theme.html\": " + e.Message);

                return;
            }

            Console.WriteLine("Processing " + files.Length + " files");

            String CategoriesText = "";

            for (int i = 0; i < files.Length; i++)
            {
                String BaseName = files[i].Substring(FromDirectory.Length + 1);
                BaseName = BaseName.Substring(0, BaseName.LastIndexOf("."));

                String OutName = ToDirectory + "/" + BaseName + ".html";

                Console.WriteLine("... " + files[i] + "(as " + OutName + ")");

                try
                {
                    StreamReader In = new StreamReader(files[i]);

                    String Content = In.ReadToEnd();

                    if (BaseName.ToUpper() == "CATEGORIES")
                    {
                        CategoriesText = Processor.Transform(Content);
                    }
                    else
                    {
                        FileInfo finfo = new FileInfo();
                        finfo.BaseName = BaseName;
                        finfo.Text = Content;

                        FileCache.Add(BaseName, finfo);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to process file '" + files[i] + "': " + e.Message);

                    continue;
                }
            }

            StringBuilder SearchNames = new StringBuilder(), SearchURLs = new StringBuilder();

            foreach (KeyValuePair<String, FileInfo> pair in FileCache)
            {
                SearchNames.Append((SearchNames.Length > 0 ? ", " : "") + "\"" + pair.Key + "\"\n");
                SearchURLs.Append((SearchURLs.Length > 0 ? ", " : "") + "\"" + pair.Value.BaseName + ".html\"\n");
            }

            for (int i = 0; i < files.Length; i++)
            {
                String BaseName = files[i].Substring(FromDirectory.Length + 1);
                BaseName = BaseName.Substring(0, BaseName.LastIndexOf("."));

                String OutName = ToDirectory + "/" + BaseName + ".html";

                if (BaseName.ToUpper() == "CATEGORIES")
                    continue;

                FileInfo finfo = FileCache[BaseName];
                String OutText = Processor.Transform(finfo.Text);

                int Index = 0;

                try
                {
                    StreamWriter Out = new StreamWriter(OutName);

                    String FinalText = (String)ThemeText.Clone();

                    Index = FinalText.IndexOf("{TITLE}");

                    while (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + Processor.Transform(BasePageTitle + ": " + BaseName.Replace("_", " ")).Replace("<p>", "").Replace("</p>", "") + FinalText.Substring(Index + "{TITLE}".Length);

                        Index = FinalText.IndexOf("{TITLE}");
                    };

                    Index = FinalText.IndexOf("{SEARCHNAMES}");

                    while (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + SearchNames + FinalText.Substring(Index + "{SEARCHNAMES}".Length);

                        Index = FinalText.IndexOf("{SEARCHNAMES}");
                    };

                    Index = FinalText.IndexOf("{SEARCHURLS}");

                    while (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + SearchURLs + FinalText.Substring(Index + "{SEARCHURLS}".Length);

                        Index = FinalText.IndexOf("{SEARCHURLS}");
                    };

                    Index = FinalText.IndexOf("{CONTENT}");

                    if (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + OutText + FinalText.Substring(Index + "{CONTENT}".Length);
                    };

                    Index = FinalText.IndexOf("{CATEGORIES}");

                    if (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + CategoriesText + FinalText.Substring(Index + "{CATEGORIES}".Length);
                    };

                    Out.Write(FinalText);
                    Out.Flush();
                    Out.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to process file '" + files[i] + "': " + e.Message);

                    continue;
                }
            }

            Console.WriteLine("OK!");
        }
    }
}
