# Introduction

Static Wiki is a pure HTML [Markdown](http://www.markdowntutorial.com/)-based wiki which can be read without additional software, and even while offline.

# How does it look?

![](Screenshots/Screenshot1.png?raw=true)

![](Screenshots/Screenshot2.png?raw=true)

# Why should I use this?

Static Wiki's sources for creating the HTML pages are regular Markdown text files. You can use them anywhere else and they will work the same way.

You can also make backups more easily. Want to use `.zip` files? [Git](http://www.github.com) repositories? Post directly to the web?

The web files generated use a base theme that shouldn't require any online resources to display properly, so you can take your files with you and present them to friends or work on your things without requiring an internet connection.

Did I mention it'll look an awful lot better than just a plain text file?

# Instructions

Copy the `Content` folder from the latest release and edit the `staticwiki.ini` file to change your wiki file.

Then, start the `StaticWikiHelper` app, and open the folder you created. You should open the folder that contains the `staticwiki.ini` file, not a sub-folder.

From then on, while `StaticWikiHelper` is open, it will constantly generate your HTML files in the output folder (`staticwiki` by default) by reading the `.md` files in your source folder (`pagesources` by default)
based on the theme file (`staticwikitheme/theme.html` by default).

Finally, the `Navigation.md` file can be used to customize the page navigation of your wiki. You can create links one line at a time in a form of `Name=URL` per line. An example would be something like `Google=http://www.google.com`.

# Notes

Some Markdown extensions are enabled by default, such as [piped tables](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet#tables). Right now there is no possibility to change which are enabled by default,
but in a future release that will be doable.

Processed pages will have the same file extension as the theme file.

The `Title` option in `staticwiki.ini` is the base page title - The pages generated will have a title in the form of `Title - Current Page Title`.

Current Page Title will have "_"'s removed.

Special sections in themes are:

- `{TITLE}` - should be placed on the &lt;title&gt; tag
- `{CONTENT}` - should be placed where you want the page content to show
- `{SEARCHNAMES}` - A list of javascript strings containing the page names
- `{SEARCHADDRESSES}` - A list of javascript strings containing the page addresses
- `{BEGINNAV}` - Begins a code snippet for navigation
- `{ENDNAV}` - Ends a code snippet for navigation
- `{NAVNAME}` - The name of the navigation item
- `{NAVLINK}` - The link of the navigation item

# License

MIT

# Contact

[littlecodingfox.com](http://www.littlecodingfox.com) &nbsp;&middot;&nbsp;
GitHub [@LittleCodingFox](https://github.com/LittleCodingFox) &nbsp;&middot;&nbsp;
Twitter [@LittleCodingFox](https://twitter.com/LittleCodingFox)
