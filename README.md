_Note: Now that XNA Game Studio will feature content compression (starting in XNA Game Studio 3.0, currently available as a Beta), development on EasyZip has come to an end. I will not be upgrading the project to 3.0 nor making any additional changes to it. I highly recommend utilizing the new compression feature of XNA Game Studio 3.0 instead of this solution as it will provide a much more stable solution to content sizes._

EasyZip is an incredibly easy way to automatically take your built XNA Game Studio content, compress it into a ZIP archive file, and read your content back out without having to unzip the whole thing! EasyZip integrates seamlessly into the build experience so you never have to update any list of items for the zip file or manage console commands. Simply run your project through the updater and you are ready to start programming with the EasyZip library.

Or if you choose not to automatically build your content into ZIP files, the ZipContentManager is able to read any Windows-created ZIP file. So feel free to manually ZIP up your XNB files into one or more ZIP files and then read the data right out using the ZipContentManager.

Without compressing your content, games can easily become huge downloads solely on content size. PNG image files can increase in size from 100-200kb for your source PNG file into a 1-1.5MB XNB file. With EasyZip you can place all these XNB files into a ZIP file which can reduce them in size by 50-80%. This lets you use more content for your games without worrying about file sizes.

Audio files can also be put into the ZIP files. The custom ZipContentManager class has the ability to load an AudioEngine, SoundBank, and WaveBank stored inside. This can help take the cut down those massive XACT projects by a huge amount.
