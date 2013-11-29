using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MarkdownSharp;

namespace StaticWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("StaticWiki started with no options. The options available are:");
                Console.WriteLine("StaticWiki -from FromDirectory -to ToDirectory -theme themefile -title pagetitle");
                Console.WriteLine();
                Console.WriteLine("From Directory should contain multiple .txt files that contain Markdown code");
                Console.WriteLine("Theme file should contain a HTML file with special section keywords to replace with page contents");
                Console.WriteLine("Special sections are:");
                Console.WriteLine("\t\t{TITLE}");
                Console.WriteLine("\t\t{CONTENT}");
                Console.WriteLine("Page Title is actually base page title - Page title will actually be \"PageTitle - CurrentPageTitle\"");
                Console.WriteLine("Current Page Title will have \"_\"'s removed");
            }

            String FromDirectory = ".", ToDirectory = "./Out/", ThemeFile = "Theme.html", BasePageTitle = "TEMPLATE";

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
                    ThemeFile = args[i + 1];
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
            Console.WriteLine("Theme File: \"" + ThemeFile + "\"");
            Console.WriteLine("Base Page Title: \"" + BasePageTitle + "\"");

            String ThemeText = "";

            try
            {
                StreamReader In = new StreamReader(ThemeFile);

                ThemeText = In.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read theme file \"" + ThemeFile + "\": " + e.Message);

                return;
            }

            Console.WriteLine("Processing " + files.Length + " files");

            for (int i = 0; i < files.Length; i++)
            {
                String BaseName = files[i].Substring(FromDirectory.Length + 1);
                BaseName = BaseName.Substring(0, BaseName.LastIndexOf("."));

                String OutName = ToDirectory + "/" + BaseName + ".html";

                Console.WriteLine("... " + files[i] + "(as " + OutName + ")");

                try
                {
                    StreamReader In = new StreamReader(files[i]);
                    StreamWriter Out = new StreamWriter(OutName);

                    String OutText = Processor.Transform(In.ReadToEnd());

                    int Index = 0;

                    for (; ; )
                    {
                        Index = OutText.IndexOf("/Content/");

                        if (Index == -1)
                            break;

                        String Link = OutText.Substring(Index + "/Content/".Length);
                        int LinkLength = OutText.IndexOf("\"", Index) - Index;

                        if (Link.IndexOf(".") != -1)
                        {
                            Link = Link.Substring(0, Link.IndexOf(".")) + ".html";
                        }
                        else
                        {
                            Link = Link.Substring(0, Link.IndexOf("\""));
                        }

                        OutText = OutText.Substring(0, Index) + Link + OutText.Substring(Index + LinkLength);
                    }

                    String FinalText = (String)ThemeText.Clone();

                    Index = FinalText.IndexOf("{TITLE}");

                    if (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + Processor.Transform(BasePageTitle + ": " + BaseName.Replace("_", " ")).Replace("<p>", "").Replace("</p>", "") + FinalText.Substring(Index + "{TITLE}".Length);
                    };

                    Index = FinalText.IndexOf("{CONTENT}");

                    if (Index != -1)
                    {
                        FinalText = FinalText.Substring(0, Index) + OutText + FinalText.Substring(Index + "{CONTENT}".Length);
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
