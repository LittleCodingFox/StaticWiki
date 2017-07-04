using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticWiki
{
    public class StaticWikiCore
    {
        private const string NavigationFileName = "Navigation";
        private const string SourceFilesExtension = "md";

        private class FileInfo
        {
            public string baseName;
            public string text;
        }

        private static string MarkdownStrippedString(string markdownString, MarkdownPipeline pipeline)
        {
            return Markdown.ToHtml(markdownString.Replace("_", " "), pipeline).Replace("<p>", "").Replace("</p>", "");
        }

        private static void HandleTitleTag(ref string finalText, string processedTitle)
        {
            var index = finalText.IndexOf("{TITLE}");

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + processedTitle + finalText.Substring(index + "{TITLE}".Length);

                index = finalText.IndexOf("{TITLE}");
            }
        }

        private static void HandleSearchTags(ref string finalText, string searchNames, string searchURLs)
        {
            var index = finalText.IndexOf("{SEARCHNAMES}");

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + searchNames + finalText.Substring(index + "{SEARCHNAMES}".Length);

                index = finalText.IndexOf("{SEARCHNAMES}");
            }

            index = finalText.IndexOf("{SEARCHURLS}");

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + searchURLs + finalText.Substring(index + "{SEARCHURLS}".Length);

                index = finalText.IndexOf("{SEARCHURLS}");
            }
        }

        private static void HandleNavTags(ref string finalText, List<KeyValuePair<string, string>> navigationInfo)
        {
            var beginNavIndex = -1;
            var endNavIndex = -1;

            var index = finalText.IndexOf("{BEGINNAV}");

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + finalText.Substring(index + "{BEGINNAV}".Length);
                beginNavIndex = index;
            }

            index = finalText.IndexOf("{ENDNAV}");

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + finalText.Substring(index + "{ENDNAV}".Length);
                endNavIndex = index;

                if (beginNavIndex != -1)
                {
                    var clonedText = finalText.Substring(beginNavIndex, endNavIndex - beginNavIndex);
                    var processedText = new StringBuilder(finalText.Substring(0, beginNavIndex));

                    for (var j = 0; j < navigationInfo.Count; j++)
                    {
                        var navItemText = clonedText.Replace("{NAVNAME}", navigationInfo[j].Key).Replace("{NAVLINK}", navigationInfo[j].Value);

                        processedText.Append(navItemText);
                    }

                    processedText.Append(finalText.Substring(endNavIndex));

                    finalText = processedText.ToString();
                }
            }
        }

        private static void HandleContentTag(ref string finalText, string contentText)
        {
            var index = finalText.IndexOf("{CONTENT}");

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + contentText + finalText.Substring(index + "{CONTENT}".Length);
            }
        }

        private static List<KeyValuePair<string, string>> ProcessNavigation(string content)
        {
            var outNavigation = new List<KeyValuePair<string, string>>();
            var lines = content.Split("\n".ToCharArray());

            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                var pieces = line.Split("=".ToCharArray());

                if (pieces.Length != 2)
                {
                    Console.WriteLine("[Navigation] Invalid line '{0}': Expecting format 'Name=Link'", line);

                    continue;
                }

                outNavigation.Add(new KeyValuePair<string, string>(pieces[0].Replace("\n", "").Replace("\r", "").Trim(),
                    pieces[1].Replace("\n", "").Replace("\r", "").Trim()));
            }

            return outNavigation;
        }

        public static string ProcessFile(string sourceText, string themeText, string title, List<KeyValuePair<string, string>> navigationInfo, string[] searchNames, string[] searchURLs)
        {
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var searchNamesString = string.Join(",", searchNames.Select(x => string.Format("\"{0}\"", MarkdownStrippedString(x, pipeline).Replace("\n", ""))).ToArray());
            var searchURLsString = string.Join(",", searchURLs.Select(x => string.Format("\"{0}\"", x)).ToArray());
            var contentText = Markdown.ToHtml(sourceText, pipeline);
            var processedTitle = Markdown.ToHtml(title.Replace("_", " "), pipeline).Replace("<p>", "").Replace("</p>", "");

            var finalText = (string)themeText.Clone();

            HandleTitleTag(ref finalText, processedTitle);
            HandleSearchTags(ref finalText, searchNamesString, searchURLsString);
            HandleNavTags(ref finalText, navigationInfo);
            HandleContentTag(ref finalText, contentText);

            return finalText;
        }

        public static bool CopyThemeResourcesToFolder(string themeFileName, string destinationDirectory, ref string logMessage)
        {
            var files = new string[0];
            var themeDirectory = Path.GetDirectoryName(themeFileName);

            try
            {
                files = Directory.GetFiles(themeDirectory, "*.*", SearchOption.AllDirectories);

                foreach(var file in files)
                {
                    if(file == Path.GetFullPath(themeFileName))
                    {
                        continue;
                    }

                    var baseName = file.Substring(themeDirectory.Length + 1);
                    var outName = Path.Combine(destinationDirectory, baseName);

                    var directory = Path.GetDirectoryName(outName);

                    if(!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(file, outName);
                }
            }
            catch(Exception)
            {
                logMessage += string.Format("StaticWiki failed to copy some theme resources to '{0}'", destinationDirectory);

                return false;
            }

            return true;
        }

        public static bool ProcessDirectory(string sourceDirectory, string destinationDirectory, string themeFileName, string navigationFileName, string baseTitle, ref string logMessage)
        {
            var fileCache = new Dictionary<string, FileInfo>();
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var files = new string[0];
            logMessage = "";

            try
            {
                files = Directory.GetFiles(sourceDirectory, string.Format("*.{0}", SourceFilesExtension), SearchOption.AllDirectories);
            }
            catch (Exception)
            {
                logMessage += string.Format("StaticWiki failed to find files at the directory '{0}'\n", sourceDirectory);

                return false;
            }

            var themeText = "";

            try
            {
                StreamReader In = new StreamReader(themeFileName);

                themeText = In.ReadToEnd();
            }
            catch (Exception e)
            {
                logMessage += string.Format("Failed to read theme file \"{0}\": {1}\n", themeFileName, e.Message);

                return false;
            }

            var pageExtension = Path.GetExtension(themeFileName);

            try
            {
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
            }
            catch (Exception)
            {
            }

            CopyThemeResourcesToFolder(themeFileName, destinationDirectory, ref logMessage);

            var navigationInfo = new List<KeyValuePair<string, string>>();

            try
            {
                var content = File.ReadAllText(navigationFileName);

                navigationInfo = ProcessNavigation(content);
            }
            catch (Exception)
            {
                logMessage += string.Format("Failed to read navigation info from '{0}'", navigationFileName);
            }

            logMessage += string.Format("Processing {0} files\n", files.Length);

            foreach(var file in files)
            {
                var baseName = file.Substring(sourceDirectory.Length + 1);
                baseName = baseName.Substring(0, baseName.LastIndexOf("."));

                var fileName = Path.GetFileName(baseName);

                if (baseName.IndexOf(fileName) > 0)
                {
                    var subdirectoryName = baseName.Substring(0, baseName.Length - fileName.Length - 1);

                    baseName = string.Format("{0}_({1})", baseName.Substring(subdirectoryName.Length + 1), subdirectoryName.Replace(" ", "_").Replace("\\", "_").Replace("/", "_"));
                }

                var outName = Path.Combine(destinationDirectory, baseName + pageExtension);

                try
                {
                    var inReader = new StreamReader(file);
                    var content = inReader.ReadToEnd();

                    inReader.Close();

                    var fileInfo = new FileInfo();
                    fileInfo.baseName = baseName;
                    fileInfo.text = content;

                    fileCache.Add(baseName, fileInfo);
                }
                catch (Exception e)
                {
                    logMessage += string.Format("Failed to process file '{0}': {1}\n", file, e.Message);

                    continue;
                }
            }

            var searchNames = new List<string>();
            var searchURLs = new List<string>();

            foreach (var pair in fileCache)
            {
                searchNames.Add(pair.Key);
                searchURLs.Add(pair.Value.baseName);
            }

            for (int i = 0; i < files.Length; i++)
            {
                var baseName = files[i].Substring(sourceDirectory.Length + 1);
                baseName = baseName.Substring(0, baseName.LastIndexOf("."));

                var fileName = Path.GetFileName(baseName);

                if (baseName.IndexOf(fileName) > 0)
                {
                    var subdirectoryName = baseName.Substring(0, baseName.Length - fileName.Length - 1);

                    baseName = string.Format("{0}_({1})", baseName.Substring(subdirectoryName.Length + 1), subdirectoryName.Replace(" ", "_").Replace("\\", "_").Replace("/", "_"));
                }

                var outName = Path.Combine(destinationDirectory, baseName + pageExtension);

                if (!fileCache.ContainsKey(baseName))
                    continue;

                logMessage += string.Format("... {0} (as {1})\n", files[i], outName);

                var fileInfo = fileCache[baseName];
                var processedText = ProcessFile(fileInfo.text, (string)themeText.Clone(), string.Format("{0}: {1}", baseTitle, baseName), navigationInfo, searchNames.ToArray(), searchURLs.ToArray());

                try
                {
                    var outWriter = new StreamWriter(outName);
                    outWriter.Write(processedText);
                    outWriter.Flush();
                    outWriter.Close();
                }
                catch (Exception e)
                {
                    logMessage += string.Format("Failed to process file '{0}': {1}\n", files[i], e.Message);
                }
            }

            logMessage += "Done\n";

            return true;
        }
    }
}
