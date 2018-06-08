# Blogger2Ghost.NET

I personally used this project to move my blog from Blogger to Ghost. I tried using a number of 
existing migration tools, but none did everything I wanted and all were written in a language I 
don't know well enough to quickly fix their shortcomings.

## Features

 - Use the blogger export feature and don't rely on the public atom/rss feeds.
 - Remap blogger tags to ghost tags (incl redirects)
 - Remap multiple blogger tags to a single ghost tag
 - Delete redundant tags
 - Auto-order tags based on priority
 - Remap old urls to new ones (incl redirects)
 - Convert HTML to Markdown
 - Download all images from blogger (and other sources)
 - Detect missing/broken images
 - Detect malformed links
 - Retain the old RSS feed urls
 - Retain the Google+ comments
 - Fixes internal links
 - Converts `<pre>` tags to code blocks
 - Generate eady to import zip file

## Uses

This project relies on 

 - ManyConsole to provide rudimentary help information
 - Newtonsoft.Json to build the mapping and export files
 - ReverseMarkdown to convert HTML to Markdown

## Sample conversion

 1. Download [blogger backup from blogger admin panel.](https://www.blogger.com/blogger.g#othersettings)
 2. Generate all the mapping files
    * Run 
       ```
      blogger2ghost mapping -i bloggerbackup.xml -o .\migration --all
      ```
       * Include `--include-drafts` to migrate draft posts as well.
       * Include `--from` and `--to` to change the urls of each post using Regex.
       * Include `--force` to overwrite any existing mapping file.
       * Replace `--all` with `--tags`, `--urls` or `--authors` to (re)generate individual files.
    * Run 
       ```
      blogger2ghost images -i bloggerbackup.xml -o .\migration
      ```
       * Include `--include-drafts` to migrate draft posts as well.
       * Include `--force` to overwrite any existing mapping file.
 3. Create the required users in Ghost and set their email and slug.
 4. Manually edit the `authors.json` to map your Google+ account to your Ghost email and slug.
 5. Manually edit the `tags.json` to map your blogger tags to ghost tags.
    * Map multiple tags by adding them to the blogger tags array.
    * If the blogger tag is different than the ghost slug also add the blogger tag to the aliases array.
    * Create parent/child tags by nesting child tags in the child_tags array.
 6. Manually edit the `urls.json` to update the slug (ToUrl) to use on Ghost (don't change the blogger urls).
 7. Manually edit the `images.json` to update image file names or external locations
    * If you want to rename your images, change the file name in the `.\migration\images` folder and update the `images.json` accordingly.
    * If images failed to download optionally download alternate images yourself and update the `.\migration\images` accordingly.
 8. Optionally resize your images using your favorite image editor.
 9. Upload your custom theme containing the `custom-blogger.hbs` template.
 9. At this point you're ready to generate the import file for Ghost.
    
    ```
    blogger2ghost convert -i bloggerbackup.xml -o .\migration 
    ```    

    Optionally add:
     * Include `--force` to overwrite any existing export files.
     * Include `--template blogger` to assign the `custom-blogger.hbs` template to each post
     * Include `--zip` to zip up the `.\migration\ghost.json` and all images in `.\migration\images` for import.
     * Include `--markdown` to convert your blogger HTML to markdown.
     * Include `--include-drafts` to migrate draft posts as well.
     * Include `--redirect-permanent` to generate 301 type redirects.
 10. Upload the final `ghost.json` or `ghost.zip` to your ghost blog.
 11. Upload the final `redirects.json` to your ghost blog.
 
## Trying again

It took me a few attempts to get everything right. You can use the "Delete all content" option in the Ghost admin panel to start over.

## Google+ Comments

To include Google+ comments from your old blog, include:

```
<script src="https://apis.google.com/js/plusone.js"></script>
<div id="comments"></div>
<script>
    if (bloggerUrl){
		gapi.comments.render('comments', {
    		href: bloggerUrl,
    		width: '624',
    		first_party_property: 'BLOGGER',
    		view_type: 'FILTERED_POSTMOD'
		});
    }
</script>
```

in your `custom-blogger.hbs` in the comment section. The `bloggerUrl` variable will automatically be added in the post's code injection header section.
