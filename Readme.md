Winamp Now Playing to File
===

This is a plugin for [Winamp](http://www.winamp.com/) that saves text information and album art for the currently playing song to files on your computer.

You can customize where the files are saved, as well as the format of the text. When Winamp is not playing, the text file will be empty, and the album art image file will be deleted. When a song is playing but it does not contain any album art in its metadata, the image file will be a 1px × 1px opaque black PNG.

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="true" levels="1,2" -->

- [Problem](#problem)
- [Solution](#solution)
- [Installation](#installation)
- [Configuration](#configuration)
- [Integration](#integration)
- [Uninstallation](#uninstallation)

<!-- /MarkdownTOC -->

<a id="problem"></a>
## Problem

I was broadcasting video game streams on [my Twitch.tv channel](https://twitch.tv/aldaviva), in which I also play music in the background. I wanted viewers to be able to tell which song I was playing at any given time in case they liked it and wanted to find it for themselves. I started using the [Advanced mIRC Integration Plug-In (AIMP)](http://amip.tools-for.net/wiki/), which is generally used for showing your Now Playing status in IRC using [mIRC](https://www.mirc.com/). It also lets you save the status to a text file, which I added as a Text Source in [OBS](https://obsproject.com/). Unfortunately, AIMP only supports encoding the text using ANSI, OEM (DOS), FIDO, or KOI8 character encodings, none of which are UTF-8, which OBS requires. For example, `Jävla Sladdar` by [Etnoscope](https://www.discogs.com/Etnoscope-Way-Over-Deadline/master/284523) was being shown in the video stream as `J�vla Sladdar`, because even though AMIP was saving `ä` using ANSI (`0xe4`), OBS was decoding the file with UTF-8, so the character was not properly decoded.

<a id="solution"></a>
## Solution

I wrote my own Winamp plugin to save information about the currently playing song to a UTF-8 text file, and the song's album art to an image file.

<a id="installation"></a>
## Installation

1. Ensure you have the [Microsoft .NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework) Runtime or later installed.
1. Download the [latest release](https://github.com/Aldaviva/WinampNowPlayingToFile/releases) ZIP file (not the source code file).
1. Extract the archive to your Winamp installation directory.
    ```
    C:\Program Files (x86)\Winamp
    ├── plugins
    │   └── gen_WinampNowPlayingToFile.dll
    ├── WinampNowPlayingToFile.dll
    ├── Daniel15.Sharpamp.dll
    └── mustache-sharp.dll
    └── taglib-sharp.dll
    ```
1. Restart Winamp if it's already running.

<a id="configuration"></a>
## Configuration

<a id="text"></a>
### Text

By default, this plugin saves textual information about the currently playing song to `winamp-now-playing.txt` in your temporary directory (`%TEMP%`), and the file contains the track Artist, Title, and Album (if applicable), for example
```text
U2 - Exit - The Joshua Tree
```

To customize the text file location and contents,

1. Start Winamp.
1. In Winamp's Preferences, go to Plug-ins › General Purpose.
1. Configure the Now Playing to File plugin.
1. You can change the file contents by editing the **Text template** and inserting placeholders for the following song information.
	- `{{Album}}`
	- `{{Artist}}`
	- `{{Filename}}`
	- `{{Title}}`
	- `{{Year}}`
1. As you fill in the template, the **Text preview** will be updated to show how the currently-playing song would be rendered, or if no song is playing, an example song.
1. Advanced template logic can be added using [Handlebars expressions](https://handlebarsjs.com/), including the [built-in helpers](https://handlebarsjs.com/guide/builtin-helpers.html) like `{{#if}}`, `{{#else}}`, `{{/if}}`, and `{{#unless}}`. To insert a line break (CRLF), use `{{#newline}}`.
1. You can change where the file is written on your drive by selecting a different path for **Save text as**.
1. Click OK to write your settings to the registry (`HKCU\Software\WinampNowPlayingToFile`).

<a id="album-art-image"></a>
### Album art image

This plugin also saves the currently playing song's album art to `winamp-now-playing.png` in the same directory at the original format and resolution as it is saved in the song metadata tags.

Note that the file extension is not changed, even if the album art has a file type different from PNG, to make it easier to refer to this file from other programs like OBS without having to deal with multiple possible file extensions. This means that this file may be a JPEG with a `.png` file extension. Most programs, including OBS, can handle this case just fine, but it is a little silly looking.

You can customize the album art filename and path using  **Save album art as** in the same configuration dialog as the text file above.

<a id="integration"></a>
## Integration

<a id="obs"></a>
### OBS

1. Start playing a song in Winamp.
1. Create a new Text (GDI+) source in your OBS scene.
1. In the Properties for your text source, enable Read From File.
1. Select the text file created by this plugin (by default, `%TEMP%\winamp-now-playing.txt`).
1. Create a new Image source in your scene.
1. In the Properties for your image source, select the image file created by this plugin (by default, `%TEMP%\winamp-now-playing.png`).

<a id="uninstallation"></a>
## Uninstallation

1. In Winamp's Preferences, go to Plug-ins › General Purpose.
1. Select the Now Playing to File plugin, then click the Uninstall Selected Plug-In button.
1. Delete all the files you had extracted to the Winamp installation directory when installing this plugin.
    ```
    C:\Program Files (x86)\Winamp
    ├── plugins
    │   └── gen_WinampNowPlayingToFile.dll
    ├── WinampNowPlayingToFile.dll
    ├── Daniel15.Sharpamp.dll
    ├── mustache-sharp.dll
    └── taglib-sharp.dll
    ```
1. Delete the song information files (by default, `winamp-now-playing.txt` and `winamp-now-playing.png` in `%TEMP%`).