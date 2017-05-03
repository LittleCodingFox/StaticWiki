# StaticWiki

Static is a pure HTML Markdown-based wiki.

It runs as a C# console application processing several .txt files and merging them with a theme file

# Instructions

Place your .txt files in a directory, choose an output directory, and a theme file with special sections for page title and content. You can find an example in the Themes folder.

Then, run the generator with your chosen options and it'll output several files on the out directory with everything merged.
	
You should have a Navigation.txt file that will contain the navigation content for the theme.
That file will be mixed together with the other .txt files.

# Options

StaticWiki -from FromDirectory -to ToDirectory -theme themefolder -title pagetitle

From Directory should contain multiple .txt files that contain Markdown code

Theme file should specify a text file file with special section keywords to replace with page contents.

Processed pages will have the same extension as the theme file.

Title is the base page title - It will become be "Title - Current Page Title"

Current Page Title will have "_"'s removed

Special sections are:

- {TITLE} - should be placed on the &lt;title&gt; tag
- {CONTENT} - should be placed where you want the page content to show
- {SEARCHNAMES} - A list of javascript strings containing the page names
- {SEARCHADDRESSES} - A list of javascript strings containing the page addresses
- {BEGINNAV} - Begins a code snippet for navigation
- {ENDNAV} - Ends a code snippet for navigation
- {NAVNAME} - The name of the navigation item
- {NAVLINK} - The link of the navigation item
