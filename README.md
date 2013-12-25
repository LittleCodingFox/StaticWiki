StaticWiki
==========

Static (as in, pure HTML, no PHP/ASP/Editable-on-browser) Markdown-based wiki

Runs as a C# console application processing several .txt files and merging them with a theme file

Instructions
============

Place several .txt files in a directory, choose an Out directory, and a theme .html file with special sections for page title and content.
	
You should have a Categories.txt file that will contain the category content for the theme's {CATEGORIES} section.
That file should be mixed together with the other .txt files, and will be ignored for conversion to .html files.

Then, run the generator with your chosen options and it'll output several .html files on the out directory with everything merged.

Options
=======

	StaticWiki -from FromDirectory -to ToDirectory -theme themefolder -title pagetitle

	From Directory should contain multiple .txt files that contain Markdown code

	Theme folder should contain a theme.html file with special section keywords to replace with page contents

	Special sections are:

		{TITLE} - should be placed on the <title> tag

		{CONTENT} - should be placed where you want the page content to show

		{CATEGORIES} - should be placed where you want the category listings to show

	Page Title is actually base page title - Page title will actually be "PageTitle - CurrentPageTitle"

	Current Page Title will have "_"'s removed
