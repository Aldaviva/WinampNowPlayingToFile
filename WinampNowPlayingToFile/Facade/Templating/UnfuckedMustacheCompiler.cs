#nullable enable

using Mustache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WinampNowPlayingToFile.Facade.Templating;

public interface UnfuckedTemplateCompiler {

    event EventHandler<UnfuckedPlaceholderFoundEventArgs>? placeholderFound;
    event EventHandler<VariableFoundEventArgs>? variableFound;

    bool areExtensionTagsAllowed { get; set; }
    bool removeNewLines { get; set; }

    UnfuckedGenerator compile(string template);

}

public class UnfuckedMustacheCompiler: UnfuckedTemplateCompiler {

    private static readonly Regex JSON_TEMPLATE_PATTERN = new("""^{{#jsonObject(?:\s+(?<keys>\w+))*\s*}}$""");

    public event EventHandler<UnfuckedPlaceholderFoundEventArgs>? placeholderFound;
    public event EventHandler<VariableFoundEventArgs>? variableFound;

    private readonly FormatCompiler mustacheCompiler = new();

    public UnfuckedMustacheCompiler() {
        mustacheCompiler.PlaceholderFound += (sender, e) => placeholderFound?.Invoke(sender, new UnfuckedPlaceholderFoundEventArgs(e));
        mustacheCompiler.VariableFound    += (sender, e) => variableFound?.Invoke(sender, e);
    }

    public bool areExtensionTagsAllowed {
        get => mustacheCompiler.AreExtensionTagsAllowed;
        set => mustacheCompiler.AreExtensionTagsAllowed = value;
    }

    public bool removeNewLines {
        get => mustacheCompiler.RemoveNewLines;
        set => mustacheCompiler.RemoveNewLines = value;
    }

    public UnfuckedGenerator compile(string template) {
        Match jsonMatch = JSON_TEMPLATE_PATTERN.Match(template);
        if (jsonMatch.Success) {
            List<string> propertyNames = jsonMatch.Groups["keys"].Captures.Cast<Capture>().Select(key => key.Value).ToList();
            foreach (string propertyName in propertyNames) {
                placeholderFound?.Invoke(this, new UnfuckedPlaceholderFoundEventArgs(propertyName, null, null, []));
            }
            return new JsonObjectGenerator(propertyNames);
        } else {
            return new UnfuckedMustacheGenerator(mustacheCompiler.Compile(template));
        }
    }

}