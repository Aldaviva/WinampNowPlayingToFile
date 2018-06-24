Winamp Now Playing to File
===

This is a plugin for [Winamp](http://www.winamp.com/) that saves information about the currently playing song to a file on your computer.

You can customize where the file is saved, as well as the format of the information. When Winamp is not playing, the file will be empty.

## Problem

I was broadcasting video game streams on [my Twitch.tv channel](https://twitch.tv/aldaviva), in which I also play music in the background. I wanted viewers to be able to tell which song I was playing at any given time, in case they liked it and wanted to find it for themselves. I started using the [Advanced mIRC Integration Plug-In (AIMP)](http://amip.tools-for.net/wiki/), which is generally used for showing your Now Playing status in IRC using [mIRC](https://www.mirc.com/). It also lets you save the status to a text file, which I added as a Text Source in [OBS](https://obsproject.com/). Unfortunately, AIMP only supports encoding the text using ANSI, OEM (DOS), FIDO, or KOI8 character encodings, none of which are UTF-8, which OBS requires. For example, `Jävla Sladdar` by [Etnoscope](https://www.discogs.com/Etnoscope-Way-Over-Deadline/master/284523) was being shown in the video stream as `J�vla Sladdar`, because even though AMIP was saving `ä` using ANSI, OBS was decoding the file with UTF-8, so the character was not properly decoded.

## Solution

I wrote my own Winamp plugin to save information about the currently playing song to a text file, encoded with UTF-8.

## Installation

1. Download the latest release
2. Extract the archive to your Winamp installation directory
    ```
    C:\Program Files (x86)\Winamp
    ├── plugins
    │   └── gen_WinampNowPlayingToFile.dll
    ├── WinampNowPlayingToFile.dll
    ├── Daniel15.Sharpamp.dll
    └── mustache-sharp.dll
    ```

3. Restart Winamp if it's already running

## Configuration

By default, this plugin saves information about the currently playing song to `winamp-now-playing.txt` in your temporary directory (`%TEMP%`), and the file contains the track Artist, Title, and Album (if applicable), for example
```text
U2 - Exit - The Joshua Tree
```

To customize the file location and contents,

1. Start Winamp
2. In Winamp's Preferences, go to Plug-ins › General Purpose
3. Configure Now Playing to File
4. You can change the file contents by editing the template and inserting template placeholders for the following track information
	- `{{Album}}`
	- `{{Artist}}`
	- `{{Filename}}`
	- `{{Title}}`
	- `{{Year}}`
5. Advanced template logic can be added using [Handlebars expressions](https://handlebarsjs.com/), including the [built-in helpers](https://handlebarsjs.com/builtin_helpers.html) like `{{#if}}` and `{{#unless}}`
6. You can change where the file is written on your drive
7. Click Save to write your settings to the registry (`HKCU\Software\WinampNowPlayingToFile`)

## Usage

### OBS

1. Start playing a song in Winamp
2. Create a new Text (GDI+) source in your OBS scene
3. In the Properties for your source, enable Read From File
4. Select the text file created by this plugin (by default, `%TEMP%\winamp-now-playing.txt`)