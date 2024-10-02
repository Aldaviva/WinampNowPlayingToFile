#nullable enable

using Mustache;
using System;

namespace WinampNowPlayingToFile.Facade.Templating;

public class UnfuckedKeyNotFoundEventArgs(string key, string? missingMember, bool isExtension): EventArgs {

    private readonly KeyNotFoundEventArgs? wrapped;

    public UnfuckedKeyNotFoundEventArgs(KeyNotFoundEventArgs wrapped): this(wrapped.Key, wrapped.MissingMember, wrapped.IsExtension) {
        this.wrapped = wrapped;
    }

    public string key { get; } = key;
    public string? missingMember { get; } = missingMember;
    public bool isExtension { get; } = isExtension;

    private bool internalHandled;
    public bool handled {
        get => wrapped?.Handled ?? internalHandled;
        set {
            if (wrapped != null) {
                wrapped.Handled = value;
            } else {
                internalHandled = value;
            }
        }
    }

    private object? internalSubstitute;
    public object? substitute {
        get => wrapped?.Substitute ?? internalSubstitute;
        set {
            if (wrapped != null) {
                wrapped.Substitute = value;
            } else {
                internalSubstitute = value;
            }
        }
    }

}