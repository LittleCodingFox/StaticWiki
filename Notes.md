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

