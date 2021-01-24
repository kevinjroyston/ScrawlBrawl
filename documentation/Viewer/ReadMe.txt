Upgrading the viewer

Run the unity BUILD to a separate (temporary) folder.

in RoystonGame\src\WebFrontEnd\ClientApp\src\viewer 

delete the Build and TemplateData folders.

Copy the Build and TemplateData folders from your temporary build into the repository \viewer folder.

Note: the index.html file in \viewer has been updated, do not copy the new index.html created with the new build.
In the unlikely event that there are changes to index.html, they must be merged into the existing index.html