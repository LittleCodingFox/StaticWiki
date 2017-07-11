using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private const string pageTitleThemeTag = "{PAGETITLE}";
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

        /// <summary>
        /// Contains details on a wiki page file
        /// </summary>
        private class FileInfo
        {
            /// <summary>
            /// The page's title
            /// </summary>
            public string pageTitle;

            /// <summary>
            /// The name of this page
            /// </summary>
            public string baseName;

            /// <summary>
            /// The name of this page formatted into a file name
            /// </summary>
            public string saneBaseName;

            /// <summary>
            /// The text of this page
            /// </summary>
            public string text;
        }

        /// <summary>
        /// Strips a Markdown string by removing fake whitespace ("_") and removing Paragraph (<p>) tags
        /// </summary>
        /// <param name="markdownString">The string to strip</param>
        /// <param name="pipeline">The markdown pipeline to use</param>
        /// <returns>The stripped string</returns>
        private static string MarkdownStrippedString(string markdownString, MarkdownPipeline pipeline)
        {
            return Markdown.ToHtml(markdownString.Replace("_", " "), pipeline).Replace("<p>", "").Replace("</p>", "");
        }

        /// <summary>
        /// Formats a category name into an proper search name by prefixing it with the category prefix and replacing directory separations into "_"
        /// </summary>
        /// <param name="categoryName">The name of the category</param>
        /// <returns>The formatted category name</returns>
        private static string FormattedCategoryName(string categoryName)
        {
            return string.Format("{0}{1}", categoryPrefix, categoryName.Replace("\\", "/").Replace("/", "_").Replace(" ", "_"));
        }

        /// <summary>
        /// Sanitizes a file name by replacing invalid characters with "_"'s
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The sane file name</returns>
        private static string SaneFileName(string fileName)
        {
            string invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars().Where(x => x != '/' && x != '\\').ToArray()));
            string invalidRegexString = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidCharacters);

            return Regex.Replace(fileName, invalidRegexString, "_");
        }

        /// <summary>
        /// Handles the Title tag for the current output text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="processedTitle">The title we are to use</param>
        private static void HandleTitleTag(ref string finalText, string processedTitle)
        {
            var index = finalText.IndexOf(titleThemeTag);

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + processedTitle + finalText.Substring(index + titleThemeTag.Length);

                index = finalText.IndexOf(titleThemeTag);
            }
        }

        /// <summary>
        /// Handles the Page Title tag for the current output text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="processedTitle">The title we are to use</param>
        private static void HandlePageTitleTag(ref string finalText, string processedTitle)
        {
            var index = finalText.IndexOf(pageTitleThemeTag);

            while (index != -1)
            {
                finalText = finalText.Substring(0, index) + processedTitle + finalText.Substring(index + pageTitleThemeTag.Length);

                index = finalText.IndexOf(pageTitleThemeTag);
            }
        }

        /// <summary>
        /// Handles the Search tags for the current output text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="searchNames">The names of the search pages as a single string</param>
        /// <param name="searchURLs">The URLs of the search pages as a single string</param>
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

        /// <summary>
        /// Handles navigation details for the navigation bar for the current output text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="navigationInfo">The navigation details using a list of Key Value Pairs where the Key is the Name and the Value is the URL</param>
        private static void HandleNavTags(ref string finalText, List<KeyValuePair<string, string>> navigationInfo)
        {
            var beginNavIndex = -1;
            var endNavIndex = -1;

            var index = finalText.IndexOf(beginNavigationThemeTag);

            if (index == -1)
            {
                return;
            }

            finalText = finalText.Substring(0, index) + finalText.Substring(index + beginNavigationThemeTag.Length);
            beginNavIndex = index;

            index = finalText.IndexOf(endNavigationThemeTag);

            if (index == -1)
            {
                return;
            }

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

        /// <summary>
        /// Handles the Content tag for the current output text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="contentText">Our content text</param>
        private static void HandleContentTag(ref string finalText, string contentText)
        {
            var index = finalText.IndexOf(contentThemeTag);

            if (index != -1)
            {
                finalText = finalText.Substring(0, index) + contentText + finalText.Substring(index + contentThemeTag.Length);
            }
        }

        /// <summary>
        /// Handles the tags for a category page
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="baseName">The name of this page</param>
        /// <param name="categoriesInfo">The details of the pages in this category, stored as a Key Value Pair, where the Key is the name and the Value is the URL</param>
        /// <param name="isCategories">Whether we are actually a category page</param>
        private static void HandleCategoryPageTags(ref string finalText, string baseName, List<KeyValuePair<string, string>> categoriesInfo, bool isCategories)
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

        /// <summary>
        /// Handles the Category tags in a page
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="baseName">The name of this page</param>
        /// <param name="categoriesInfo">The details of the pages in this category, stored as a Key Value Pair, where the Key is the name and the Value is the URL</param>
        /// <param name="isCategories">Whether we are actually a category page</param>
        private static void HandleCategoryTags(ref string finalText, string baseName, List<KeyValuePair<string, string>> categoriesInfo, bool isCategories)
        {
            HandleCategoryPageTags(ref finalText, baseName, categoriesInfo, isCategories);

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

        /// <summary>
        /// Handles the root directory tag from the current finalized text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="currentDirectory">Our current directory</param>
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

        /// <summary>
        /// Handles the basename tag from the current finalized text
        /// </summary>
        /// <param name="finalText">Our current finalized text</param>
        /// <param name="baseName">The base name of the current page</param>
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

        /// <summary>
        /// Processes links in content by validating whether they exist and replacing them properly or marking them as invalid
        /// </summary>
        /// <param name="content">The content text</param>
        /// <param name="sourceDirectory">Our current source directory</param>
        /// <param name="currentDirectory">Our current directory</param>
        /// <param name="pageExtension">Our page extension</param>
        /// <param name="searchURLs">Our search URLs</param>
        /// <returns>The processed links</returns>
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
                    var filePath = url.Contains("://") ? url : Path.Combine(sourceDirectory, currentDirectory, SaneFileName(url));

                    if (!url.Contains("://") && !Directory.Exists(filePath) && !File.Exists(filePath))
                    {
                        var recursiveBack = currentDirectory.Length > 0 ?
                            string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";

                        if (searchURLs.Where(x => x.EndsWith(SaneFileName(url))).Any())
                        {
                            url = recursiveBack + SaneFileName(searchURLs.Where(x => x.EndsWith(SaneFileName(url))).FirstOrDefault()) + pageExtension;
                        }
                        else
                        {
                            invalid = true;
                        }

                        if (invalid)
                        {
                            content = content.Replace(linkMatch.Groups[0].Value, string.Format("<a href=\"#\">{0}</a>", linkMatch.Groups[2].Value));
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

        /// <summary>
        /// Processes the navigation file
        /// </summary>
        /// <param name="content">The file's contents</param>
        /// <returns>The processed navigation data</returns>
        private static List<KeyValuePair<string, string>> ProcessNavigationFileContent(string content, ref string logMessage)
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
                    logMessage += string.Format("[Navigation] Invalid line '{0}': Expecting format 'Name=Link'", line);

                    continue;
                }

                var navName = pieces[0].Replace("\n", "").Replace("\r", "").Trim();
                var navLink = string.Join("=", pieces.Skip(1).ToArray()).Replace("\n", "").Replace("\r", "").Trim();

                outNavigation.Add(new KeyValuePair<string, string>(navName, navLink));
            }

            return outNavigation;
        }

        /// <summary>
        /// Processes a page file and returns its processed contents
        /// </summary>
        /// <param name="baseName">The page name</param>
        /// <param name="sourceText">The source text</param>
        /// <param name="themeText">The theme text</param>
        /// <param name="title">The page's title with the wiki title prepended</param>
        /// <param name="pageTitle">The page's title</param>
        /// <param name="navigationInfo">The Navigation Information</param>
        /// <param name="searchNames">The names of all searchable pages</param>
        /// <param name="searchURLs">The URLs of all searchable pages</param>
        /// <param name="categoriesInfo">The Category Information for this page</param>
        /// <param name="sourceDirectory">Our source directory</param>
        /// <param name="currentDirectory">Our currrent directory</param>
        /// <param name="pageExtension">Our page extension</param>
        /// <param name="isCategories">Whether we're a category page</param>
        /// <returns>The processed page contents</returns>
        public static string ProcessFile(string baseName, string sourceText, string themeText, string title, string pageTitle, List<KeyValuePair<string, string>> navigationInfo,
            string[] searchNames, string[] searchURLs, List<KeyValuePair<string, string>> categoriesInfo, string sourceDirectory, string currentDirectory, string pageExtension,
            bool isCategories)
        {
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var recursiveBack = currentDirectory.Length > 0 ?
                string.Join("", currentDirectory.Replace("\\", "/").Split("/".ToCharArray()).Select(x => "../")) : "";
            var searchNamesString = string.Join(",", searchNames.Select(x => string.Format("\"{0}\"", MarkdownStrippedString(x, pipeline).Replace("\n", ""))).ToArray());
            var searchURLsString = string.Join(",", searchURLs.Select(x => string.Format("\"{0}{1}{2}\"", recursiveBack, x, pageExtension)).ToArray());
            var contentText = Markdown.ToHtml(sourceText, pipeline);
            var processedTitle = MarkdownStrippedString(title, pipeline);

            var finalText = (string)themeText.Clone();

            HandleTitleTag(ref finalText, processedTitle);
            HandlePageTitleTag(ref finalText, pageTitle);
            HandleSearchTags(ref finalText, searchNamesString, searchURLsString);
            HandleNavTags(ref finalText, navigationInfo);
            HandleCategoryTags(ref finalText, baseName, categoriesInfo, isCategories);
            HandleBasenameTag(ref finalText, baseName);
            HandleRootDirectoryTag(ref finalText, currentDirectory);
            HandleContentTag(ref finalText, contentText);

            finalText = ProcessLinksInContent(finalText, sourceDirectory, currentDirectory, pageExtension, searchURLs);

            return finalText;
        }

        /// <summary>
        /// Copies the resources from a theme folder to a destination directory
        /// </summary>
        /// <param name="themeFileName">The theme's file name</param>
        /// <param name="destinationDirectory">Our destination directory</param>
        /// <param name="logMessage">Our current log message</param>
        /// <returns>Whether we successfully copied the files</returns>
        private static bool CopyThemeResourcesToFolder(string themeFileName, string destinationDirectory, ref string logMessage)
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

        /// <summary>
        /// Deletes a directory
        /// </summary>
        /// <param name="destinationDirectory">The directory to delete</param>
        private static void DeleteDirectory(string destinationDirectory)
        {
            var files = Directory.GetFiles(destinationDirectory);
            var directories = Directory.GetDirectories(destinationDirectory);

            foreach (var file in files)
            {
                File.Delete(file);
            }

            foreach (var directory in directories)
            {
                DeleteDirectory(directory);
            }

            Directory.Delete(destinationDirectory, false);
        }

        /// <summary>
        /// Processes a source directory into a destination directory
        /// </summary>
        /// <param name="sourceDirectory">The source directory</param>
        /// <param name="destinationDirectory">The destination directory</param>
        /// <param name="themeFileName">The file name of the theme file</param>
        /// <param name="navigationFileName">The file name of the navigation file</param>
        /// <param name="contentExtensions">The file extensions for all content files</param>
        /// <param name="baseTitle">The wiki title</param>
        /// <param name="logMessage">Our current log message</param>
        /// <returns>Whether we successfully processed the source into the destination</returns>
        public static bool ProcessDirectory(string sourceDirectory, string destinationDirectory, string themeFileName, string navigationFileName, string[] contentExtensions, string baseTitle, ref string logMessage)
        {
            var fileCache = new Dictionary<string, FileInfo>();
            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseBootstrap().Build();
            var files = new string[0];
            var categoriesDictionary = new Dictionary<string, List<string>>();
            var categoriesRegex = new Regex("\\[categories\\](.*?)\\[\\/categories\\]");
            var titleRegex = new Regex("\\[title\\](.*?)\\[\\/title\\]");

            logMessage = "";

            try
            {
                DeleteDirectory(destinationDirectory);
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
                var inReader = new StreamReader(themeFileName);

                themeText = inReader.ReadToEnd();
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

                navigationInfo = ProcessNavigationFileContent(content, ref logMessage);
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
                    var title = "";

                    inReader.Close();

                    foreach (Match match in categoriesRegex.Matches(content))
                    {
                        if (match.Groups.Count == 2 && (match.Groups[0].Index == 0 || content[match.Groups[0].Index - 1] == '\n'))
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

                    foreach (Match match in titleRegex.Matches(content))
                    {
                        if (match.Groups.Count == 2 && (match.Groups[0].Index == 0 || content[match.Groups[0].Index - 1] == '\n'))
                        {
                            title = match.Groups[1].Value;
                            content = content.Replace(match.Groups[0].Value, "");
                        }
                    }

                    var isCategoryPage = SaneFileName(baseName).StartsWith(SaneFileName(categoryPrefix));

                    if(isCategoryPage && title.Length == 0)
                    {
                        title = baseName.Replace(SaneFileName(categoryPrefix), categoryPrefix);
                    }

                    var fileInfo = new FileInfo();
                    fileInfo.pageTitle = title.Length > 0 ? title : baseName;
                    fileInfo.baseName = baseName;
                    fileInfo.saneBaseName = SaneFileName(baseName);
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
                searchNames.Add(pair.Value.pageTitle);
                searchURLs.Add(SaneFileName(pair.Value.baseName));
            }

            foreach (var categoryName in categoriesDictionary.Keys)
            {
                var formattedCategoryName = FormattedCategoryName(categoryName);
                var saneCategoryName = SaneFileName(formattedCategoryName);

                //Make sure category lists are created
                if (!fileCache.Where(x => x.Value.saneBaseName == SaneFileName(formattedCategoryName)).Any())
                {
                    fileCache.Add(formattedCategoryName, new FileInfo()
                    {
                        pageTitle = formattedCategoryName,
                        baseName = formattedCategoryName,
                        saneBaseName = saneCategoryName,
                        text = ""
                    });

                    searchNames.Add(formattedCategoryName);
                    searchURLs.Add(saneCategoryName);
                }
            }

            foreach (var file in fileCache.Keys)
            {
                var baseName = file;
                var fileName = Path.GetFileName(baseName);
                var directoryName = Path.GetDirectoryName(baseName);
                var saneBaseName = SaneFileName(baseName);
                var outName = Path.Combine(destinationDirectory, SaneFileName(baseName) + pageExtension);

                logMessage += string.Format("... {0} (as {1})\n", file, outName);

                var categoryInfo = new List<KeyValuePair<string, string>>();
                var isCategoryPage = saneBaseName.StartsWith(SaneFileName(categoryPrefix));

                //If we're a category page and our base name doesn't match, then that means we're a generated category page.
                //If there is another file whose baseName is equal to our sane base name, then there exists a page that should override the generated one. Skip this one.
                if(isCategoryPage && baseName != saneBaseName && fileCache.Where(x => x.Value.baseName == saneBaseName).Any())
                {
                    continue;
                }

                if(isCategoryPage)
                {
                    var unformattedCategoryName = saneBaseName.Replace(SaneFileName(categoryPrefix), "");

                    if (categoriesDictionary.ContainsKey(unformattedCategoryName))
                    {
                        var items = categoriesDictionary[unformattedCategoryName];

                        foreach(var item in items)
                        {
                            var cacheInfo = fileCache.Where(x => x.Value.saneBaseName == SaneFileName(item)).Select(x => x.Value).FirstOrDefault();
                            var itemTitle = cacheInfo != null ? cacheInfo.pageTitle : null;

                            if(itemTitle != null && itemTitle != cacheInfo.saneBaseName && itemTitle != cacheInfo.baseName)
                            {
                                categoryInfo.Add(new KeyValuePair<string, string>(itemTitle, SaneFileName(item)));
                            }
                            else
                            {
                                var lastPiece = item.Replace("\\", "/").Split("/".ToCharArray()).Last().Replace("_", " ");

                                categoryInfo.Add(new KeyValuePair<string, string>(lastPiece, SaneFileName(item)));
                            }
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
                var processedText = ProcessFile(baseName, fileInfo.text, (string)themeText.Clone(), string.Format("{0}: {1}", baseTitle, fileInfo.pageTitle),
                    fileInfo.pageTitle, navigationInfo, searchNames.ToArray(), searchURLs.ToArray(), categoryInfo, sourceDirectory, directoryName,
                    pageExtension, isCategoryPage);

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

        /// <summary>
        /// Gets details on a workspace from a workspace directory by parsing the "staticwiki.ini" file
        /// </summary>
        /// <param name="workspaceDirectory">The workspace's directory</param>
        /// <param name="sourceDirectory">The parsed source directory</param>
        /// <param name="destinationDirectory">The parsed destination directory</param>
        /// <param name="themeFileName">The parsed theme file name</param>
        /// <param name="titleName">The parsed wiki title name</param>
        /// <param name="navigationFileName">The parsed navigation file name</param>
        /// <param name="contentExtensions">The parsed content extensions</param>
        /// <param name="logMessage">Our current log mesasge</param>
        /// <returns>Whether we successfully parsed the staticwiki.ini file</returns>
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
