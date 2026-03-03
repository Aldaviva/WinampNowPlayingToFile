using Daniel15.Sharpamp;
using System.Text.RegularExpressions;

namespace WinampNowPlayingToFile.Facade;

internal static class Extensions {

    extension(Song source) {

        public Data.Song Abstract() => new() {
            Artist   = source.Artist,
            Album    = source.Album,
            Title    = source.Title,
            Year     = parseYear(source.Year),
            Filename = source.Filename
        };

    }

    private static int? parseYear(string rawYear) {
        if (int.TryParse(rawYear, out int year)) {
            return year;
        } else if (Regex.Match(rawYear, @"(?<year>\d{4})-\d\d-\d\d") is { Success: true } isoDateMatch) {
            return int.Parse(isoDateMatch.Groups["year"].Value);
        } else {
            return null;
        }
    }

    extension(Status source) {

        public Data.Status Abstract() => (Data.Status) source;

    }

}