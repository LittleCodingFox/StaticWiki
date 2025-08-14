[title]Notes and Documentation[/title]
[toc]
[template name="toc"]
[templateitem]
[templateitemtag name="page-link"]#page-titles[/templateitemtag]
[templateitemtag name="page-name"]Page Titles[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#linking-pages[/templateitemtag]
[templateitemtag name="page-name"]Linking Pages[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#categories[/templateitemtag]
[templateitemtag name="page-name"]Categories[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#navigation[/templateitemtag]
[templateitemtag name="page-name"]Navigation[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#custom-html-and-images[/templateitemtag]
[templateitemtag name="page-name"]Custom HTML and Images[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#markdown-extensions[/templateitemtag]
[templateitemtag name="page-name"]Markdown Extensions[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#templates[/templateitemtag]
[templateitemtag name="page-name"]Templates[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#active-page[/templateitemtag]
[templateitemtag name="page-name"]Active Page[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#toc[/templateitemtag]
[templateitemtag name="page-name"]TOC[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#using-themes[/templateitemtag]
[templateitemtag name="page-name"]Using Themes[/templateitemtag]
[/templateitem]

[templateitem]
[templateitemtag name="page-link"]#theme-sections[/templateitemtag]
[templateitemtag name="page-name"]Theme Sections[/templateitemtag]
[/templateitem]
[/template]
[/toc]

# Page Titles

The `Title` option in `staticwiki.ini` is the base page title - The pages generated will have a title in the form of `Title - Current Page Title`.

Current Page Title will have `_`'s replaced with spaces, so a page like `Page_Sample` will become `Page Sample`.

Pages can have a custom title by using the `[title]` tag. For example, `[title]My title[/title}`.

# Linking Pages

Processed pages will have the same file extension as the theme file.

You can link to other pages by specifying their path as if you were in the root directory.
So if I have my wiki at a specific spot, and I want to link to `Sample/Markdown_Sample.md`, I just need to do something like `[my link](Sample/Markdown_Sample)`.
If a non-URL link has no file extension and it is a page that already exists, it'll automatically get the proper file extension.

Page links will automatically receive subdirectory correction. For instance, `Sample/Markdown_Sample` will become `../Sample/Markdown_Sample` if we're in the `Markdown_Sample` page in the `Sample` directory.
Also, theme files use the `{ROOT}` tag for the same purpose.

This ensures that the wiki is navigationable regardless of the location of the current page, without duplicating any files.

However, pages must be linked the same way across all directories. So if you're in `Markdown_Sample.md` file located in the `Sample` directory and you want to link to another page in the `Sample` directory,
you still have to link to `Sample/Page_Name`.

# Categories

Categories can be added to a page by adding the `[categories]` tag at the start of the page. For example, `[categories]Sample, Markdown[/categories]`.
Additionally, you can create category pages in your sources (with their name in the form of `Category_name`) to add some content to your category page.

Category pages are always created in the root directory of your wiki (the destination folder where your wiki is generated).
So if you want to add custom content to a category page, you must create that file in the root directory.

You can also make text in the theme or page vary based on one or more categories using the `[categorycontent]` tag.

It works in the form of `[categorycontent categories=a, b, c, d]content goes here[/categorycontent]`. An empty category list makes it add the content if the page has no categories.

# Navigation

Regular navigation can be done using a `Navigation.list` file at the workspace directory (the same directory that has your `staticwiki.ini` file), where each line corresponds to a name and a link, separated by a `=`. For example:

```
Google=http://www.google.com
```

However, if you need more advanced navigation, you can create a `Navigation.md` file in the same place as `Navigation.list`, and StaticWiki will favor the Markdown file over the list file.

# Custom HTML and images

You can use custom HTML for displaying your content, although you can't combine it with Markdown as you might expect.
Essentially, you can add HTML to your Markdown, separate from the Markdown itself. You cannot combine them at the same time.

So, for instance, you can do:

```
<strong><a href="http://www.google.com">Google</a></strong>
```

But not:

```
<strong>[Google](http://www.google.com)</strong>
```

For adding an image in a custom way, such as a custom size, you can do simple HTML like:

```
<img src="my image.png" width="my width" height="my height">
```

Or:

```
<img src="my image.png" style="border:1; width:my width; height: my height">
```

# Markdown Extensions

You can use several [Markdig](https://github.com/lunet-io/markdig) extensions by using the `Extensions` field in `staticwiki.ini`.
Extensions are separated by a comma and can have spaces. So if you want to use the `Bootstrap` and `Pipe Tables` extensions, you'd add this:

```
MarkdownExtensions=Bootstrap, Pipe Tables
```

These are the available extensions:

- `Bootstrap`
- `Pipe Tables`
- `Grid Tables`
- `Grid Tables`
- `Extra Emphasis`
- `Special Attributes`
- `Definition Lists`
- `Footnotes`
- `Auto Identifiers`
- `Auto Links`
- `Task Lists`
- `Extra Bullet Lists`
- `Media Support`
- `Abbreviations`
- `Citations`
- `Custom Containers`
- `Figures`
- `Footers`
- `Mathematics`
- `Hardline Breaks`
- `Emoji`
- `Smarty Pants`
- `Diagrams`
- `YAML Frontmatter`

You can find more details on this in the [Markdig](https://github.com/lunet-io/markdig) page.

# Templates

You can create templates in a theme, and use them in a page, both in navigation and the page content.

Templates can have tags and items, and each item can have tags. Tags can have multiple lines.

Any other content in a template item is added as content of the template item with the `{TEMPLATEITEMCONTENT}` theme tag.

You can find an example of a template in the default theme file and in the [Templates](Templates) sample page.

# Active Page

You can add content if a page is active using the `[activepage name="Your page name here"]your content goes here[/activepage]` tag.

You can also add a link to a page from anywhere using the `[pagelink]page name[/pagelink]` tag, which will automatically use `../` when needed to make sure it's in the right folder.

# TOC

You can make a table of contents section for a page by adding a `[toc]contents[/toc]` tag to it. The contents you put in it will be applied in the `{TOC}` section of your theme, if that theme supports it.

# Using themes

To use themes, you must specify the themes in the `staticwiki.ini` file. You must specify the default theme name as well as a list of names and paths.

```
DefaultThemeName=Default
Themes=Default:staticwikitheme/theme.html
```

To override a specific theme in a page, you can use the `[theme]theme name[/theme]` tag.

# Theme sections

Special sections in themes are:

- `{TITLE}` - should be placed on the &lt;title&gt; tag
- `{CONTENT}` - should be placed where you want the page content to show
- `{SEARCHNAMES}` - A list of javascript strings containing the page names
- `{SEARCHADDRESSES}` - A list of javascript strings containing the page addresses
- `{BEGINNAV}` - Begins a code snippet for navigation
- `{ENDNAV}` - Ends a code snippet for navigation
- `{NAVNAME}` - The name of the navigation item
- `{NAVLINK}` - The link of the navigation item
- `{ROOT}` - Root folder indicator for theme files
- `{BEGINPAGECATEGORIES}` - Begins a code snippet for inserting a page's categories
- `{ENDPAGECATEGORIES}` - Ends a code snippet for a page's categories
- `{BEGINCATEGORYLIST}` - Begins the list of a category page's contents
- `{ENDCATEGORYLIST}` - Ends the list of a category page's contents
- `{CATEGORYNAME}`- The name of a category
- `{CATEGORYLINK}` - The link to a category
- `{BEGINIFCATEGORIES}` - Begins a code snippet if the page has categories
- `{ENDIFCATEGORIES}` - Ends a code snippet if the page has categories
- `{BASENAME}` - The name of the current file
- `{PAGETITLE}` - The title of the current file
- `{TEMPLATEINDEX}` - The index of a template in a page.
- `{TEMPLATEITEMINDEX}` - The index of a template item inside a template.
- `{TEMPLATEITEMCONTENT}` - The content of a template item is put here.
