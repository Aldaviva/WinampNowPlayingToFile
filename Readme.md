Winamp Now Playing to File
===

[![Build status](https://img.shields.io/github/actions/workflow/status/Aldaviva/WinampNowPlayingToFile/msbuild.yml?branch=master&logo=github)](https://github.com/Aldaviva/WinampNowPlayingToFile/actions/workflows/msbuild.yml) [![Test status](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:WinampNowPlayingToFile/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/206386) [![Coverage status](https://img.shields.io/coveralls/github/Aldaviva/WinampNowPlayingToFile?logo=coveralls)](https://coveralls.io/github/Aldaviva/WinampNowPlayingToFile?branch=master)

This is a plugin for [Winamp](http://www.winamp.com/) that saves text information and album art for the currently playing song to files on your computer. You can customize where the files are saved, as well as the format of the text.

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2" -->

- [Problem](#problem)
- [Solution](#solution)
- [Installation](#installation)
- [Configuration](#configuration)
- [Integration](#integration)
- [Uninstallation](#uninstallation)

<!-- /MarkdownTOC -->

## Problem

I was broadcasting video game streams on [my Twitch.tv channel](https://twitch.tv/aldaviva), in which I also play music in the background. I wanted viewers to be able to tell which song I was playing at any given time in case they liked it and wanted to find it for themselves. I started using the [Advanced mIRC Integration Plug-In (AMIP)](http://amip.tools-for.net/wiki/), which is generally used for showing your Now Playing status in IRC using [mIRC](https://www.mirc.com/). It also lets you save the status to a text file, which I added as a Text Source in [OBS](https://obsproject.com/).

Unfortunately, AMIP only supports encoding the text using ANSI, OEM (DOS), FIDO, or KOI8 character encodings, none of which are UTF-8, which OBS requires. For example, [Jävla Sladdar](https://www.youtube.com/watch?v=zaCZ9VkJ-so) was being shown in the video stream as `J�vla Sladdar`, because even though AMIP was saving `ä` using ANSI (`0xe4`), OBS was decoding the file with UTF-8, so the character was not properly decoded. In UTF-8, `ä` is supposed to be encoded as `0xc3 0xa4` because `0xe4` is greater than `0x7f` (i.e. `0x34` requires more than 7 bits to represent), so it spills over into a second code unit (byte).

## Solution

I wrote my own Winamp plugin to save information about the currently playing song to a UTF-8 text file, and the song's album art to an image file.

## Installation

1. Ensure you have [Microsoft .NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework) Runtime or later installed. This is included in Windows 10 version 1803 and later.
1. Exit Winamp if it's already running.
1. Download [**`WinampNowPlayingToFile.zip`**](https://github.com/Aldaviva/WinampNowPlayingToFile/releases/latest/download/WinampNowPlayingToFile.zip) from the [latest release](https://github.com/Aldaviva/WinampNowPlayingToFile/releases) (not the source code ZIP file).
1. Extract the archive to your Winamp installation directory.
    ```
    📁 C:\Program Files (x86)\Winamp
    ├── 📁 plugins
    │   └── 📄 gen_WinampNowPlayingToFile.dll
    ├── 📄 WinampNowPlayingToFile.dll
    ├── 📄 Daniel15.Sharpamp.dll
    ├── 📄 mustache-sharp.dll
    └── 📄 taglib-sharp.dll
    ```

## Configuration

Configuration of this plugin is performed in Winamp.

1. Start Winamp.
1. Go to Options › Preferences › Plug-ins › General Purpose.
1. Configure the Now Playing to File plugin.

![configuration](.github/images/configuration.png)

*Changes are saved to the registry in `HKCU\Software\WinampNowPlayingToFile`.*

### Text

By default, this plugin saves textual information about the currently playing song to `winamp_now_playing.txt` in your user's temporary directory (`%TEMP%`), and the file contains the track Artist, Title, and Album (if applicable), for example
```text
U2 - Exit - The Joshua Tree
```

To customize the text file location and contents, go to the plugin preferences in Winamp.

1. You can change the file contents by editing the **Text template** and inserting placeholders for the following song information.
	- `{{Album}}`
	- `{{Artist}}`
	- `{{Filename}}`
	- `{{Title}}`
	- `{{Year}}`
1. As you fill in the template, the **Text preview** will be updated to show how the currently-playing song would be rendered, or if no song is playing, an example song.
1. Advanced template logic can be added using [Handlebars expressions](https://handlebarsjs.com/), including the [built-in helpers](https://handlebarsjs.com/guide/builtin-helpers.html) like `{{#if}}`, `{{#else}}`, `{{/if}}`, and `{{#unless}}`. To insert a line break (CRLF), use `{{#newline}}`. The **Insert** button lets you add these helpers to your template, or you can type them in.
1. You can change where the file is written in your filesystem by selecting a different path for **Save text as**.

When Winamp is not playing a song, this text file will be truncated to 0 bytes.

### Album art

This plugin also copies the currently playing song's album art from the song metadata or folder. By default, it is copied to `%TEMP%\winamp_now_playing.png`.

Note that the file extension is not changed, even if the album art has a file type different from PNG, to make it easier to refer to this file from other programs like OBS without having to deal with multiple possible file extensions. This means that this file may be a JPEG with a `.png` file extension. Most programs, including OBS, can handle this case just fine, but the mismatch is a little silly. Feel free to change the file extension using the preferences.

You can customize the album art filename and path using **Save album art as** in the same plugin configuration dialog as the text file above.

#### Fallback artwork

When there is no album art, the copied files will be deleted. However, this may be undesirable because it can leave dependent interfaces in a weird-looking state (like an OBS layout with a big transparent gap where the album art would normally be), and it will also trigger the Missing Files warning dialog box each time you launch OBS.

To resolve this, you can specify custom image files that will be copied instead when there is no album art. Here are some sample [black](https://placehold.co/128x128/000f/0000.png) and [transparent](https://placehold.co/128x128/0000/0000.png) images to get started, or you can use your own. There are no requirements for the format or dimensions of these images besides what your downstream consumer like OBS accepts.

##### Missing artwork

When Winamp is playing a song with no album art, the image file will be deleted. To override this, save your desired image file as `emptyAlbumArt.png` in the Winamp installation directory.

##### Playback stopped

When Winamp is paused, stopped, or closed, the image file will be deleted. To override this, save your desired image file as `stoppedAlbumArt.png` in the Winamp installation directory.

## Integration

### OBS

1. Start playing a song in Winamp.
1. Create a new Text (GDI+) source in your OBS scene.
1. In the Properties for your text source, enable Read From File.
1. Select the text file created by this plugin (by default, `%TEMP%\winamp_now_playing.txt`).
1. Create a new Image source in your scene.
1. In the Properties for your image source, select the image file created by this plugin (by default, `%TEMP%\winamp_now_playing.png`).

## Uninstallation

1. In Winamp's Preferences, go to Plug-ins › General Purpose.
1. Select the Now Playing to File plugin, then click the Uninstall Selected Plug-In button.
1. Delete all the files you extracted to the Winamp installation directory when installing this plugin.
    ```
    📁 C:\Program Files (x86)\Winamp
    ├── 📁 plugins
    │   └── 📄 gen_WinampNowPlayingToFile.dll
    ├── 📄 WinampNowPlayingToFile.dll
    ├── 📄 Daniel15.Sharpamp.dll
    ├── 📄 mustache-sharp.dll
    └── 📄 taglib-sharp.dll
    ```
1. Delete the song information files (by default, `winamp_now_playing.txt` and `winamp_now_playing.png` in `%TEMP%`).
1. Delete the plugin settings registry key `HKCU\Software\WinampNowPlayingToFile`.
