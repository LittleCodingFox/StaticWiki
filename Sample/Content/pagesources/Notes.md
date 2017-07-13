[title]Notes and Documentation[/title]

# Extensions

Some Markdown extensions are enabled by default, such as [piped tables](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet#tables). Right now there is no possibility to change which are enabled by default,
but in a future release that will be doable.

# Page Titles

The `Title` option in `staticwiki.ini` is the base page title - The pages generated will have a title in the form of `Title - Current Page Title`.

Current Page Title will have `_`'s replaced with spaces, so a page like `Page_Sample` will become `Page Sample`.

Pages can have a custom title by using the `[title]` tag. For example, `[title]My title[/title}`.

# Linking Pages

Processed pages will have the same file extension as the theme file.

You can link to other pages by specifying their path as if you were in the root directory.
So if I have my wiki at a specific spot, and I want to link to `Sample/Markdown_Sample.md`, I just need to do something like `[my link](Sample/Markdown_Sample)`.
If a non-URL link has no file extension and it is a page that already exists, it'll automatically get the proper file extension.

# Categories

Categories can be added to a page by adding the `[categories]` tag at the start of the page. For example, `[categories]Sample, Markdown[/categories]`.
Additionally, you can create category pages in your sources (with their name in the form of `Category_name`) to add some content to your category page.

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

For adding a custom image, you can do simple HTML like:

```
<img src="my image.png" width="my width" height="my height">
```

Or:

```
<img src="my image.png" style="border:1; width:my width; height: my height">
```

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

