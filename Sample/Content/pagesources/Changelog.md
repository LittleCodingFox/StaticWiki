## 1.0.0 pre5

- Added support for disabling page extensions
- Added support for disabling link correction
- Fixed an issue preventing StaticWiki from creating your wiki while in a read only directory such as directories in Google Drive
- Adjusted documentation
- Changed the Helper App so that it processes your wiki in a separate thread
- Changed categories to have the page title or file name instead of a path to the page
- Added theme support for marking broken links in a way that makes it obvious
- Fixed search URLs not using the proper page extensions
- Improved logging for better diagnosis of issues users may have

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
