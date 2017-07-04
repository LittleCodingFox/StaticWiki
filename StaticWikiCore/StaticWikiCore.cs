using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StaticWiki
{
    public class StaticWikiCore
    {
        private const string NavigationFileName = "Navigation";
        private const string SourceFilesExtension = "md";

        private const string navigationName = "Navigation.list";

        private const string configurationFileName = "staticwiki.ini";
        private const string configurationSectionName = "General";
        private const string configurationSourceDirectoryName = "SourceDir";
        private const string configurationOutputDirectoryName = "OutputDir";
        private const string configurationTitleName = "Title";
        private const string configurationThemeFileName = "ThemeFile";
        private const string configurationContentExtensionsName = "ContentExtensions";

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

        private static void HandleNavTags(ref string finalText, List<KeyValuePair<string, string>> navigationInfo, string sourceDirectory, string currentDirectory, string pageExtension, string[] searchNames)
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
                        var name = navigationInfo[j].Key;
                        var link = navigationInfo[j].Value;

                        var navItemText = clonedText.Replace("{NAVNAME}", name).Replace("{NAVLINK}", link);

                        processedText.Append(navItemText);
                    }

                    processedText.Append(finalText.Substring(endNavIndex));

                    finalText = processedText.ToString();
                    finalText = ProcessLinksInContent(finalText, sourceDirectory, currentDirectory, pageExtension, searchNames);
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

        private static void HandleRootFolderTag(ref string finalText, string currentDirectory)
        {
            var index = finalText.IndexOf("{ROOT}");
            var recursiveBack = currentDirectory.Length > 0 ?
                string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";

            while(index != -1)
            {
                finalText = finalText.Substring(0, index) + recursiveBack + finalText.Substring(index + "{ROOT}".Length);
                index = finalText.IndexOf("{ROOT}");
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

        public static string ProcessLinksInContent(string content, string sourceDirectory, string currentDirectory, string pageExtension, string[] searchNames)
        {
            var linkHrefRegex = new Regex("<a href=\"(.*?)\">((?:.(?!\\<\\/a\\>))*.)<\\/a>");

            foreach (Match linkMatch in linkHrefRegex.Matches(content))
            {
                if (linkMatch.Groups.Count == 3)
                {
                    var urlGroup = linkMatch.Groups[1];
                    var url = urlGroup.Value.Replace("\\", "/");
                    var invalid = false;

                    if (!url.Contains("://") && !Directory.Exists(Path.Combine(sourceDirectory, currentDirectory, url)) && !File.Exists(Path.Combine(sourceDirectory, currentDirectory, url)))
                    {
                        var recursiveBack = currentDirectory.Length > 0 ?
                            string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";

                        if (searchNames.Where(x => x.EndsWith(url)).Any())
                        {
                            url = recursiveBack + searchNames.Where(x => x.EndsWith(url)).FirstOrDefault() + pageExtension;
                        }
                        else
                        {
                            invalid = true;
                        }

                        if(invalid)
                        {
                            content = content.Replace(linkMatch.Groups[0].Value, string.Format("<del><a href=\"#\">{0}</a></del>", linkMatch.Groups[2].Value));
                        }
                        else
                        {
                            content = content.Replace(linkMatch.Groups[0].Value, string.Format("<a href=\"{0}\">{1}</a>", url, linkMatch.Groups[2].Value));
                        }
                    }
                }
            }

            return content;
        }

        public static string ProcessFile(string sourceText, string themeText, string title, List<KeyValuePair<string, string>> navigationInfo, string[] searchNames, string[] searchURLs,
            string sourceDirectory, string currentDirectory, string pageExtension)
        {
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var recursiveBack = currentDirectory.Length > 0 ?
                string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";
            var searchNamesString = string.Join(",", searchNames.Select(x => string.Format("\"{0}\"", MarkdownStrippedString(x, pipeline).Replace("\n", ""))).ToArray());
            var searchURLsString = string.Join(",", searchURLs.Select(x => string.Format("\"{0}{1}\"", recursiveBack, x)).ToArray());
            var contentText = ProcessLinksInContent(Markdown.ToHtml(sourceText, pipeline), sourceDirectory, currentDirectory, pageExtension, searchNames);
            var processedTitle = MarkdownStrippedString(title, pipeline);

            var finalText = (string)themeText.Clone();

            HandleTitleTag(ref finalText, processedTitle);
            HandleSearchTags(ref finalText, searchNamesString, searchURLsString);
            HandleNavTags(ref finalText, navigationInfo, sourceDirectory, currentDirectory, pageExtension, searchNames);
            HandleContentTag(ref finalText, contentText);
            HandleRootFolderTag(ref finalText, currentDirectory);

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

                    File.Copy(file, outName, true);
                }
            }
            catch(Exception)
            {
                logMessage += string.Format("StaticWiki failed to copy some theme resources to '{0}'", destinationDirectory);

                return false;
            }

            return true;
        }

        private static void DeleteDestinationContents(string destinationDirectory)
        {
            var files = Directory.GetFiles(destinationDirectory);
            var directories = Directory.GetDirectories(destinationDirectory);

            foreach(var file in files)
            {
                File.Delete(file);
            }

            foreach(var directory in directories)
            {
                DeleteDestinationContents(directory);
            }

            Directory.Delete(destinationDirectory, false);
        }

        public static bool ProcessDirectory(string sourceDirectory, string destinationDirectory, string themeFileName, string navigationFileName, string[] contentExtensions, string baseTitle, ref string logMessage)
        {
            var fileCache = new Dictionary<string, FileInfo>();
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var files = new string[0];
            logMessage = "";

            try
            {
                DeleteDestinationContents(destinationDirectory);
            }
            catch(Exception)
            {
            }

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
                baseName = baseName.Substring(0, baseName.LastIndexOf(".")).Replace("\\", "/");

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

            foreach(var file in files)
            {
                var baseName = file.Substring(sourceDirectory.Length + 1);
                baseName = baseName.Substring(0, baseName.LastIndexOf(".")).Replace("\\", "/");
                var fileName = Path.GetFileName(baseName);
                var directoryName = Path.GetDirectoryName(baseName);
                var outName = Path.Combine(destinationDirectory, baseName + pageExtension);

                if (!fileCache.ContainsKey(baseName))
                    continue;

                logMessage += string.Format("... {0} (as {1})\n", file, outName);

                var fileInfo = fileCache[baseName];
                var processedText = ProcessFile(fileInfo.text, (string)themeText.Clone(), string.Format("{0}: {1}", baseTitle, baseName), navigationInfo,
                    searchNames.ToArray(), searchURLs.ToArray(), sourceDirectory, directoryName, pageExtension);

                try
                {
                    if (directoryName.Length > 0)
                    {
                        var combinedPath = Path.Combine(destinationDirectory, directoryName);

                        if (!Directory.Exists(combinedPath))
                        {
                            Directory.CreateDirectory(combinedPath);
                        }
                    }

                    var outWriter = new StreamWriter(outName);
                    outWriter.Write(processedText);
                    outWriter.Flush();
                    outWriter.Close();
                }
                catch (Exception e)
                {
                    logMessage += string.Format("Failed to process file '{0}': {1}\n", file, e.Message);
                }
            }

            logMessage += string.Format("Copying content files (Extensions are '{0}')\n", string.Join(", ", contentExtensions.Select(x => string.Format(".{0}", x)).ToArray()));

            var contentFiles = new string[0];

            try
            {
                contentFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).Where(x => contentExtensions.Where(y => x.EndsWith(string.Format(".{0}", y))).Any()).ToArray();
            }
            catch (Exception)
            {
                logMessage += string.Format("StaticWiki failed to find content files at the directory '{0}'. This is not important and will simply be ignored.\n", sourceDirectory);
            }

            foreach(var file in contentFiles)
            {
                var baseName = file.Substring(sourceDirectory.Length + 1);
                var directoryName = Path.GetDirectoryName(baseName);
                var from = Path.Combine(sourceDirectory, baseName);
                var to = Path.Combine(destinationDirectory, baseName);

                try
                {
                    if (directoryName.Length > 0)
                    {
                        var combinedPath = Path.Combine(destinationDirectory, directoryName);

                        if (!Directory.Exists(combinedPath))
                        {
                            Directory.CreateDirectory(combinedPath);
                        }
                    }

                    File.Copy(from, to, true);
                }
                catch (Exception)
                {
                    logMessage += string.Format("Unable to copy content file '{0}' (as '{1}'\n", from, to);
                }
            }

            logMessage += "Done\n";

            return true;
        }

        public static bool GetWorkspaceDetails(string workspaceDirectory, ref string sourceDirectory, ref string destinationDirectory, ref string themeFileName, ref string titleName, ref string navigationFileName,
            ref string[] contentExtensions, ref string logMessage)
        {
            try
            {
                var configPath = Path.Combine(workspaceDirectory, configurationFileName);
                var iniParser = new Ini(configPath);

                if (iniParser.GetSections().Length == 0)
                {
                    throw new Exception(string.Format("Unable to open '{0}'\n", configPath));
                }

                sourceDirectory = iniParser.GetValue(configurationSourceDirectoryName, configurationSectionName);
                destinationDirectory = iniParser.GetValue(configurationOutputDirectoryName, configurationSectionName);
                titleName = iniParser.GetValue(configurationTitleName, configurationSectionName);
                themeFileName = iniParser.GetValue(configurationThemeFileName, configurationSectionName);

                var contentExtensionsString = iniParser.GetValue(configurationContentExtensionsName, configurationSectionName);

                if (contentExtensionsString.Length > 0)
                {
                    contentExtensions = contentExtensionsString.Split(",".ToCharArray()).Select(x => x.Trim()).ToArray();
                }
            }
            catch (Exception exception)
            {
                logMessage += string.Format("Unable to load project due to exception: {0}\n", exception);

                return false;
            }

            sourceDirectory = Path.Combine(workspaceDirectory, sourceDirectory);
            destinationDirectory = Path.Combine(workspaceDirectory, destinationDirectory);
            themeFileName = Path.Combine(workspaceDirectory, themeFileName);
            navigationFileName = Path.Combine(workspaceDirectory, navigationName);

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

            if(!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory) || !File.Exists(themeFileName))
            {
                logMessage += string.Format("Unable to load project due to invalid required files or directories\n");

                return false;
            }

            return true;
        }
    }
}
