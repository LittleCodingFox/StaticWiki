[categories]Sample[/categories]
[title]Introduction[/title]

Static Wiki is a pure HTML [Markdown](http://www.markdowntutorial.com/)-based wiki which can be read without additional software, and even while offline.

# How does it look?

You're looking at it right now!

# What's new

## 1.0.0 pre4
- Page title support and `{PAGETITLE}` support for themes
- Minor bug fixes to category pages and the search function
- Minor bug fixes to link correction so it's as generic as possible when correcting
- Fixed tags being processed together with Page Content instead of only Theme Content
- Fixed Helper app to properly detect file creation, deletion, and renaming

## 1.0.0 pre3
- Categories support

## 1.0.0 pre2

- Subdirectory support
- Automatically copy images and other content! You can edit the file types in `staticwiki.ini`
- `Navigation.list` replacing `<source directory>\Navigation.md`
- Console version that you can run from a terminal
- Automatically strikes out invalid links
- Automatically copies theme files to your output directory
- Minor bugfixes
- New sample page
- Updated UI

# Samples

You can find a markdown sample page [here](Samples/Markdown_Sample).

# Why should I use this?

Static Wiki's sources for creating the HTML pages are regular Markdown text files. You can use them anywhere else and they will work the same way.

You can also make backups more easily. Want to use `.zip` files? [Git](http://www.github.com) repositories? Post directly to the web?

The web files generated use a base theme that shouldn't require any online resources to display properly, so you can take your files with you and present them to friends or work on your things without requiring an internet connection.

Did I mention it'll look an awful lot better than just a plain text file?

# Instructions

Copy the `Content` folder from the latest [release](https://github.com/LittleCodingFox/StaticWiki/releases/latest/) and edit the `staticwiki.ini` file to change your wiki file.

Then, start the `StaticWikiHelper` app, and open the folder you created. You should open the folder that contains the `staticwiki.ini` file, not a sub-folder.

From then on, while `StaticWikiHelper` is open, it will constantly generate your HTML files in the output folder (`staticwiki` by default) by reading the `.md` files in your source folder (`pagesources` by default)
based on the theme file (`staticwikitheme/theme.html` by default).

Finally, the `Navigation.list` file (located in the same directory as `staticwiki.ini`) can be used to customize the page navigation of your wiki.
You can create links one line at a time in a form of `Name=URL` per line. An example would be something like `Google=http://www.google.com`.

# Documentation

[Find it here](Notes)

# License

MIT

# Contact

[littlecodingfox.com](http://www.littlecodingfox.com) &nbsp;&middot;&nbsp;
GitHub [@LittleCodingFox](https://github.com/LittleCodingFox) &nbsp;&middot;&nbsp;
Twitter [@LittleCodingFox](https://twitter.com/LittleCodingFox)
