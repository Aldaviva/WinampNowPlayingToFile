using System.Text.RegularExpressions;
using WinampNowPlayingToFile.Data;

namespace WinampNowPlayingToFile.Facade;

internal static class Extensions {

    public static Song Abstract(this Daniel15.Sharpamp.Song source) => new() {
        Artist   = source.Artist,
        Album    = source.Album,
        Title    = source.Title,
        Year     = parseYear(source.Year),
        Filename = source.Filename
    };

    private static int? parseYear(string rawYear) {
        if (int.TryParse(rawYear, out int year)) {
            return year;
        } else if (Regex.Match(rawYear, @"(?<year>\d{4})-\d\d-\d\d") is { Success: true } isoDateMatch) {
            return int.Parse(isoDateMatch.Groups["year"].Value);
        } else {
            return null;
        }
    }

    public static Status Abstract(this Daniel15.Sharpamp.Status source) => (Status) source;

}