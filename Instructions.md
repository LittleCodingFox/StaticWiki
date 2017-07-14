Copy the `Content` folder from the latest [release](https://github.com/LittleCodingFox/StaticWiki/releases/latest/) and edit the `staticwiki.ini` file to change your wiki file.

Then, start the `StaticWikiHelper` app, and open the folder you created. You should open the folder that contains the `staticwiki.ini` file, not a sub-folder.

From then on, while `StaticWikiHelper` is open, it will constantly generate your HTML files in the output folder (`staticwiki` by default) by reading the `.md` files in your source folder (`pagesources` by default)
based on the theme file (`staticwikitheme/theme.html` by default).

Finally, the `Navigation.list` file (located in the same directory as `staticwiki.ini`) can be used to customize the page navigation of your wiki.
You can create links one line at a time in a form of `Name=URL` per line. An example would be something like `Google=http://www.google.com`.

See the [Documentation](Notes.md) for more details.
