#nullable enable

using Mustache;
using System;

namespace WinampNowPlayingToFile.Facade.Templating;

public class UnfuckedPlaceholderFoundEventArgs(string key, string? alignment, string? formatting, Context[] context): EventArgs {

    public UnfuckedPlaceholderFoundEventArgs(PlaceholderFoundEventArgs e): this(e.Key, e.Alignment, e.Formatting, e.Context) { }

    public string key { get; } = key;
    public string? alignment { get; } = alignment;
    public string? formatting { get; } = formatting;
    public Context[] context { get; } = context;

}