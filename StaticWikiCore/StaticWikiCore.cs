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
        #region Filename Constants
        private const string navigationName = "Navigation.list";
        private const string sourceFilesExtension = "md";
        #endregion

        #region Theme Key Constants
        private const string titleThemeTag = "{TITLE}";
        private const string searchNamesThemeTag = "{SEARCHNAMES}";
        private const string searchURLsThemeTag = "{SEARCHURLS}";
        private const string beginNavigationThemeTag = "{BEGINNAV}";
        private const string endNavigationThemeTag = "{ENDNAV}";
        private const string navigationNameThemeTag = "{NAVNAME}";
        private const string navigationLinkThemeTag = "{NAVLINK}";
        private const string contentThemeTag = "{CONTENT}";
        private const string rootDirectoryThemeTag = "{ROOT}";
        private const string beginPageCategoriesThemeTag = "{BEGINPAGECATEGORIES}";
        private const string endPageCategoriesThemeTag = "{ENDPAGECATEGORIES}";
        private const string beginCategoryPageThemeTag = "{BEGINCATEGORYLIST}";
        private const string endCategoryPageThemeTag = "{ENDCATEGORYLIST}";
        private const string categoryNameThemeTag = "{CATEGORYNAME}";
        private const string categoryLinkThemeTag = "{CATEGORYLINK}";
        private const string beginIfCategoriesThemeTag = "{BEGINIFCATEGORIES}";
        private const string endIfCategoriesThemeTag = "{ENDIFCATEGORIES}";
        private const string baseNameThemeTag = "{BASENAME}";
        #endregion

        #region Misc Constants
        private const string categoryPrefix = "Category:";
        #endregion

        #region Configuration Constants
        private const string configurationFileName = "staticwiki.ini";
        private const string configurationSectionName = "General";
        private const string configurationSourceDirectoryName = "SourceDir";
        private const string configurationOutputDirectoryName = "OutputDir";
        private const string configurationTitleName = "Title";
        private const string configurationThemeFileName = "ThemeFile";
        private const string configurationContentExtensionsName = "ContentExtensions";
        #endregion

        private class FileInfo
        {
            public string baseName;
            public string text;
        }

        private static string MarkdownStrippedString(string markdownString, MarkdownPipeline pipeline)
        {
            return Markdown.ToHtml(markdownString.Replace("_", " "), pipeline).Replace("<p>", "").Replace("</p>", "");
        }

        private static string FormattedCategoryName(string categoryName)
        {
            return string.Format("{0}{1}", categoryPrefix, categoryName.Replace("\\", "/").Replace("/", "_").Replace(" ", "_"));
        }

        private static string SaneFileName(string fileName)
        {
            string invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars().Where(x => x != '/' && x != '\\').ToArray()));
            string invalidRegexString = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidCharacters);

            return Regex.Replace(fileName, invalidRegexString, "_");
        }

        private static void HandleTitleTag(ref string finalText, string processedTitle)
        {
            var index = finalText.IndexOf(titleThemeTag);

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + processedTitle + finalText.Substring(index + titleThemeTag.Length);

                index = finalText.IndexOf(titleThemeTag);
            }
        }

        private static void HandleSearchTags(ref string finalText, string searchNames, string searchURLs)
        {
            var index = finalText.IndexOf(searchNamesThemeTag);

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + searchNames + finalText.Substring(index + searchNamesThemeTag.Length);

                index = finalText.IndexOf(searchNamesThemeTag);
            }

            index = finalText.IndexOf(searchURLsThemeTag);

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + searchURLs + finalText.Substring(index + searchURLsThemeTag.Length);

                index = finalText.IndexOf(searchURLsThemeTag);
            }
        }

        private static void HandleNavTags(ref string finalText, List<KeyValuePair<string, string>> navigationInfo, string sourceDirectory, string currentDirectory, string pageExtension, string[] searchNames)
        {
            var beginNavIndex = -1;
            var endNavIndex = -1;

            var index = finalText.IndexOf(beginNavigationThemeTag);

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + finalText.Substring(index + beginNavigationThemeTag.Length);
                beginNavIndex = index;
            }

            index = finalText.IndexOf(endNavigationThemeTag);

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + finalText.Substring(index + endNavigationThemeTag.Length);
                endNavIndex = index;

                if (beginNavIndex != -1)
                {
                    var clonedText = finalText.Substring(beginNavIndex, endNavIndex - beginNavIndex);
                    var processedText = new StringBuilder(finalText.Substring(0, beginNavIndex));

                    for (var j = 0; j < navigationInfo.Count; j++)
                    {
                        var name = navigationInfo[j].Key;
                        var link = navigationInfo[j].Value.Contains("://") ? navigationInfo[j].Value : SaneFileName(navigationInfo[j].Value);

                        var navItemText = clonedText.Replace(navigationNameThemeTag, name).Replace(navigationLinkThemeTag, link);

                        processedText.Append(navItemText);
                    }

                    processedText.Append(finalText.Substring(endNavIndex));

                    finalText = processedText.ToString();
                }
            }
        }

        private static void HandleContentTag(ref string finalText, string contentText)
        {
            var index = finalText.IndexOf(contentThemeTag);

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + contentText + finalText.Substring(index + contentThemeTag.Length);
            }
        }

        private static void HandleCategoryPageTags(ref string finalText, string baseName, List<KeyValuePair<string, string>> categoriesInfo, bool isCategories, string sourceDirectory,
            string currentDirectory, string pageExtension, string[] searchNames)
        {
            var beginCategoryPageIndex = -1;
            var endCategoryPageIndex = -1;

            var index = finalText.IndexOf(beginCategoryPageThemeTag);

            if (index == -1)
            {
                return;
            }

            finalText = finalText.Substring(0, index) + finalText.Substring(index + beginCategoryPageThemeTag.Length);
            beginCategoryPageIndex = index;

            index = finalText.IndexOf(endCategoryPageThemeTag);

            if (index == -1)
            {
                return;
            }

            finalText = finalText.Substring(0, index) + finalText.Substring(index + endCategoryPageThemeTag.Length);
            endCategoryPageIndex = index;

            if(!isCategories)
            {
                finalText = finalText.Substring(0, beginCategoryPageIndex) + finalText.Substring(endCategoryPageIndex);

                return;
            }

            var clonedText = finalText.Substring(beginCategoryPageIndex, endCategoryPageIndex - beginCategoryPageIndex);

            var beginPageCategoriesIndex = -1;
            var endPageCategoriesIndex = -1;

            index = clonedText.IndexOf(beginPageCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            clonedText = clonedText.Substring(0, index) + clonedText.Substring(index + beginPageCategoriesThemeTag.Length);
            beginPageCategoriesIndex = index;

            index = clonedText.IndexOf(endPageCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            clonedText = clonedText.Substring(0, index) + clonedText.Substring(index + endPageCategoriesThemeTag.Length);
            endPageCategoriesIndex = index;

            if (beginPageCategoriesIndex != -1)
            {
                var pageCategoryClonedText = clonedText.Substring(beginPageCategoriesIndex, endPageCategoriesIndex - beginPageCategoriesIndex);
                var processedText = new StringBuilder(clonedText.Substring(0, beginPageCategoriesIndex));

                for (var j = 0; j < categoriesInfo.Count; j++)
                {
                    var name = categoriesInfo[j].Key;
                    var link = SaneFileName(categoriesInfo[j].Value);
                    var categoryItemText = pageCategoryClonedText.Replace(categoryNameThemeTag, name).Replace(categoryLinkThemeTag, link);

                    processedText.Append(categoryItemText);
                }

                processedText.Append(clonedText.Substring(endPageCategoriesIndex));

                clonedText = processedText.ToString();
            }

            finalText = finalText.Substring(0, beginCategoryPageIndex) + clonedText + finalText.Substring(endCategoryPageIndex);
        }

        private static void HandleCategoryTags(ref string finalText, string baseName, List<KeyValuePair<string, string>> categoriesInfo, bool isCategories, string sourceDirectory,
            string currentDirectory, string pageExtension, string[] searchNames)
        {
            HandleCategoryPageTags(ref finalText, baseName, categoriesInfo, isCategories, sourceDirectory, currentDirectory, pageExtension, searchNames);

            var beginIfCategoriesIndex = -1;
            var endIfCategoriesIndex = -1;
            var index = finalText.IndexOf(beginIfCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            finalText = finalText.Substring(0, index) + finalText.Substring(index + beginIfCategoriesThemeTag.Length);

            beginIfCategoriesIndex = index;

            index = finalText.IndexOf(endIfCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            finalText = finalText.Substring(0, index) + finalText.Substring(index + endIfCategoriesThemeTag.Length);
            endIfCategoriesIndex = index;

            //If we have no valid categories, skip the whole thing
            if (isCategories || categoriesInfo.Count == 0)
            {
                finalText = finalText.Substring(0, beginIfCategoriesIndex) + finalText.Substring(endIfCategoriesIndex);

                return;
            }

            var clonedText = finalText.Substring(beginIfCategoriesIndex, endIfCategoriesIndex - beginIfCategoriesIndex);

            var beginPageCategoriesIndex = -1;
            var endPageCategoriesIndex = -1;

            index = clonedText.IndexOf(beginPageCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            clonedText = clonedText.Substring(0, index) + clonedText.Substring(index + beginPageCategoriesThemeTag.Length);
            beginPageCategoriesIndex = index;

            index = clonedText.IndexOf(endPageCategoriesThemeTag);

            if (index == -1)
            {
                return;
            }

            clonedText = clonedText.Substring(0, index) + clonedText.Substring(index + endPageCategoriesThemeTag.Length);
            endPageCategoriesIndex = index;

            if (beginPageCategoriesIndex != -1)
            {
                var pageCategoryClonedText = clonedText.Substring(beginPageCategoriesIndex, endPageCategoriesIndex - beginPageCategoriesIndex);
                var processedText = new StringBuilder(clonedText.Substring(0, beginPageCategoriesIndex));

                for (var j = 0; j < categoriesInfo.Count; j++)
                {
                    var name = categoriesInfo[j].Key;
                    var link = SaneFileName(categoriesInfo[j].Value);
                    var categoryItemText = pageCategoryClonedText.Replace(categoryNameThemeTag, name).Replace(categoryLinkThemeTag, link);

                    processedText.Append(categoryItemText);
                }

                processedText.Append(clonedText.Substring(endPageCategoriesIndex));

                clonedText = processedText.ToString();
            }

            finalText = finalText.Substring(0, beginIfCategoriesIndex) + clonedText + finalText.Substring(endIfCategoriesIndex);
        }

        private static void HandleRootDirectoryTag(ref string finalText, string currentDirectory)
        {
            var index = finalText.IndexOf(rootDirectoryThemeTag);
            var recursiveBack = currentDirectory.Length > 0 ?
                string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + recursiveBack + finalText.Substring(index + rootDirectoryThemeTag.Length);
                index = finalText.IndexOf(rootDirectoryThemeTag);
            }
        }

        private static void HandleBasenameTag(ref string finalText, string baseName)
        {
            var index = finalText.IndexOf(baseNameThemeTag);
            var fileName = Path.GetFileName(baseName);

            while(index != -1)
            {
                finalText = finalText.Substring(0, index) + baseName + finalText.Substring(index + baseNameThemeTag.Length);
                index = finalText.IndexOf(baseNameThemeTag);
            }
        }

        private static string ProcessLinksInContent(string content, string sourceDirectory, string currentDirectory, string pageExtension, string[] searchURLs)
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

                        if (searchURLs.Where(x => x.EndsWith(url)).Any())
                        {
                            url = recursiveBack + SaneFileName(searchURLs.Where(x => x.EndsWith(url)).FirstOrDefault()) + pageExtension;
                        }
                        else
                        {
                            invalid = true;
                        }

                        if (invalid)
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

                if (pieces.Length < 2)
                {
                    Console.WriteLine("[Navigation] Invalid line '{0}': Expecting format 'Name=Link'", line);

                    continue;
                }

                var navName = pieces[0].Replace("\n", "").Replace("\r", "").Trim();
                var navLink = string.Join("=", pieces.Skip(1).ToArray()).Replace("\n", "").Replace("\r", "").Trim();

                outNavigation.Add(new KeyValuePair<string, string>(navName, navLink));
            }

            return outNavigation;
        }

        public static string ProcessFile(string baseName, string sourceText, string themeText, string title, List<KeyValuePair<string, string>> navigationInfo,
            string[] searchNames, string[] searchURLs, List<KeyValuePair<string, string>> categoriesInfo, string sourceDirectory, string currentDirectory, string pageExtension,
            bool isCategories)
        {
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var recursiveBack = currentDirectory.Length > 0 ?
                string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";
            var searchNamesString = string.Join(",", searchNames.Select(x => string.Format("\"{0}\"", MarkdownStrippedString(x, pipeline).Replace("\n", ""))).ToArray());
            var searchURLsString = string.Join(",", searchURLs.Select(x => string.Format("\"{0}{1}\"", recursiveBack, x)).ToArray());
            var contentText = Markdown.ToHtml(sourceText, pipeline);
            var processedTitle = MarkdownStrippedString(title, pipeline);

            var finalText = (string)themeText.Clone();

            HandleTitleTag(ref finalText, processedTitle);
            HandleSearchTags(ref finalText, searchNamesString, searchURLsString);
            HandleNavTags(ref finalText, navigationInfo, sourceDirectory, currentDirectory, pageExtension, searchNames);
            HandleCategoryTags(ref finalText, baseName, categoriesInfo, isCategories, sourceDirectory, currentDirectory, pageExtension, searchNames);
            HandleBasenameTag(ref finalText, baseName);
            HandleContentTag(ref finalText, contentText);
            HandleRootDirectoryTag(ref finalText, currentDirectory);

            finalText = ProcessLinksInContent(finalText, sourceDirectory, currentDirectory, pageExtension, searchURLs);

            return finalText;
        }

        public static bool CopyThemeResourcesToFolder(string themeFileName, string destinationDirectory, ref string logMessage)
        {
            var files = new string[0];
            var themeDirectory = Path.GetDirectoryName(themeFileName);

            try
            {
                files = Directory.GetFiles(themeDirectory, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (file == Path.GetFullPath(themeFileName))
                    {
                        continue;
                    }

                    var baseName = file.Substring(themeDirectory.Length + 1);
                    var outName = Path.Combine(destinationDirectory, baseName);

                    var directory = Path.GetDirectoryName(outName);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(file, outName, true);
                }
            }
            catch (Exception)
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

            foreach (var file in files)
            {
                File.Delete(file);
            }

            foreach (var directory in directories)
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
            var categoriesDictionary = new Dictionary<string, List<string>>();
            var categoriesRegex = new Regex("\\[categories\\](.*?)\\[\\/categories\\]");

            logMessage = "";

            try
            {
                DeleteDestinationContents(destinationDirectory);
            }
            catch (Exception)
            {
            }

            try
            {
                files = Directory.GetFiles(sourceDirectory, string.Format("*.{0}", sourceFilesExtension), SearchOption.AllDirectories);
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

            foreach (var file in files)
            {
                var baseName = file.Substring(sourceDirectory.Length + 1);
                baseName = baseName.Substring(0, baseName.LastIndexOf(".")).Replace("\\", "/");

                try
                {
                    var inReader = new StreamReader(file);
                    var content = inReader.ReadToEnd();

                    inReader.Close();

                    foreach (Match match in categoriesRegex.Matches(content))
                    {
                        if (match.Index == 0 && match.Groups.Count == 2)
                        {
                            var categoriesString = match.Groups[1].Value;
                            var categoryBits = categoriesString.Split(",".ToCharArray()).Select(x => x.Trim()).ToArray();

                            foreach (var categoryName in categoryBits)
                            {
                                if (!categoriesDictionary.ContainsKey(categoryName))
                                {
                                    categoriesDictionary.Add(categoryName, new List<string>());
                                }

                                if (!categoriesDictionary[categoryName].Contains(baseName))
                                {
                                    categoriesDictionary[categoryName].Add(baseName);
                                }
                            }

                            content = content.Replace(match.Groups[0].Value, "");
                        }
                    }

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
                searchURLs.Add(SaneFileName(pair.Value.baseName));
            }

            foreach (var categoryName in categoriesDictionary.Keys)
            {
                var formattedCategoryName = FormattedCategoryName(categoryName);

                searchNames.Add(formattedCategoryName);
                searchURLs.Add(SaneFileName(formattedCategoryName));

                //Make sure category lists are created
                if (!fileCache.Where(x => x.Key == formattedCategoryName).Any())
                {
                    fileCache.Add(formattedCategoryName, new FileInfo()
                    {
                        baseName = formattedCategoryName,
                        text = ""
                    });
                }
            }

            foreach (var file in fileCache.Keys)
            {
                var baseName = file;
                var fileName = Path.GetFileName(baseName);
                var directoryName = Path.GetDirectoryName(baseName);
                var outName = Path.Combine(destinationDirectory, SaneFileName(baseName) + pageExtension);

                logMessage += string.Format("... {0} (as {1})\n", file, outName);

                var categoryInfo = new List<KeyValuePair<string, string>>();
                var isCategoryPage = baseName.StartsWith(categoryPrefix);

                if(isCategoryPage)
                {
                    var unformattedCategoryName = baseName.Replace(categoryPrefix, "");

                    if (categoriesDictionary.ContainsKey(unformattedCategoryName))
                    {
                        var items = categoriesDictionary[unformattedCategoryName];

                        foreach(var item in items)
                        {
                            categoryInfo.Add(new KeyValuePair<string, string>(item, SaneFileName(item)));
                        }
                    }
                }
                else
                {
                    foreach (var pair in categoriesDictionary)
                    {
                        if (pair.Value.Contains(baseName))
                        {
                            categoryInfo.Add(new KeyValuePair<string, string>(pair.Key, FormattedCategoryName(pair.Key)));
                        }
                    }
                }

                categoryInfo.Sort((a, b) => a.Key.CompareTo(b.Key));

                var fileInfo = fileCache[baseName];
                var processedText = ProcessFile(baseName, fileInfo.text, (string)themeText.Clone(), string.Format("{0}: {1}", baseTitle, baseName), navigationInfo,
                        searchNames.ToArray(), searchURLs.ToArray(), categoryInfo, sourceDirectory, directoryName, pageExtension, isCategoryPage);

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

            foreach (var file in contentFiles)
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

            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory) || !File.Exists(themeFileName))
            {
                logMessage += string.Format("Unable to load project due to invalid required files or directories\n");

                return false;
            }

            return true;
        }
    }
}
