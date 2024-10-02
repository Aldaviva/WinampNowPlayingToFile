#nullable enable

using Mustache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;

namespace WinampNowPlayingToFile.Facade.Templating;

public interface UnfuckedGenerator {

    event EventHandler<UnfuckedKeyNotFoundEventArgs>? keyNotFound;
    event EventHandler<KeyFoundEventArgs>? keyFound;
    event EventHandler<TagFormattedEventArgs>? tagFormatted;
    event EventHandler<ValueRequestEventArgs>? valueRequested;

    public string render(object values, IFormatProvider? formatProvider = null);

}

public class UnfuckedMustacheGenerator: UnfuckedGenerator {

    public event EventHandler<UnfuckedKeyNotFoundEventArgs>? keyNotFound;
    public event EventHandler<KeyFoundEventArgs>? keyFound;
    public event EventHandler<TagFormattedEventArgs>? tagFormatted;
    public event EventHandler<ValueRequestEventArgs>? valueRequested;

    private readonly Generator mustacheGenerator;

    public UnfuckedMustacheGenerator(Generator mustacheGenerator) {
        this.mustacheGenerator           =  mustacheGenerator;
        mustacheGenerator.KeyFound       += (sender, args) => keyFound?.Invoke(sender, args);
        mustacheGenerator.KeyNotFound    += (sender, args) => keyNotFound?.Invoke(sender, new UnfuckedKeyNotFoundEventArgs(args));
        mustacheGenerator.TagFormatted   += (sender, args) => tagFormatted?.Invoke(sender, args);
        mustacheGenerator.ValueRequested += (sender, args) => valueRequested?.Invoke(sender, args);
    }

    public string render(object values, IFormatProvider? formatProvider = null) {
        return mustacheGenerator.Render(formatProvider, values);
    }

}

public class JsonObjectGenerator(IEnumerable<string> propertyNames): UnfuckedGenerator {

    private static readonly CultureInfo                                CULTURE                 = CultureInfo.InvariantCulture;
    private static readonly IDictionary<(Type, string), PropertyInfo?> PROPERTY_ACCESSOR_CACHE = new Dictionary<(Type, string), PropertyInfo?>();

    public event EventHandler<UnfuckedKeyNotFoundEventArgs>? keyNotFound;
    public event EventHandler<KeyFoundEventArgs>? keyFound;
    public event EventHandler<TagFormattedEventArgs>? tagFormatted;
    public event EventHandler<ValueRequestEventArgs>? valueRequested;

    public string render(object? values = null, IFormatProvider? formatProvider = null) {
        StringBuilder sb = new();

        bool isFirstField = true;
        sb.Append('{');
        foreach (string fieldName in propertyNames) {
            if (isFirstField) {
                isFirstField = false;
            } else {
                sb.Append(',');
            }

            sb.Append('"');
            sb.Append(fieldName);
            sb.Append("\":");

            object? fieldValue = values switch {
                IDictionary<string, object?> dict => dict.TryGetValue(fieldName, out object? value) ? value : onKeyNotFound(fieldName),
                IDictionary dict                  => dict.Contains(fieldName) ? dict[fieldName] : onKeyNotFound(fieldName),
                not null                          => tryGetObjectPropertyValue(values, fieldName, out object? value) ? value : onKeyNotFound(fieldName),
                _                                 => onKeyNotFound(fieldName)
            };
            sb.Append(stringify(fieldValue));
        }
        sb.Append('}');

        return sb.ToString();
    }

    private object? onKeyNotFound(string key) {
        UnfuckedKeyNotFoundEventArgs eventArgs = new(key, null, false);
        keyNotFound?.Invoke(this, eventArgs);
        return eventArgs.handled ? eventArgs.substitute : null;
    }

    private static bool tryGetObjectPropertyValue(object values, string fieldName, out object? value) {
        Type                                objectType = values.GetType();
        (Type objectType, string fieldName) cacheKey   = (objectType, fieldName);
        if (!PROPERTY_ACCESSOR_CACHE.TryGetValue(cacheKey, out PropertyInfo? propertyAccessor)) {
            propertyAccessor                  = objectType.GetProperty(fieldName);
            PROPERTY_ACCESSOR_CACHE[cacheKey] = propertyAccessor;
        }
        value = propertyAccessor?.GetValue(values);
        return propertyAccessor != null;
    }

    private static string stringify(object? value) => value switch {
        null          => "null",
        int number    => number.ToString("D", CULTURE),
        double number => number.ToString(CULTURE),
        TimeSpan time => time.TotalMilliseconds.ToString(CULTURE),
        bool boolean  => boolean ? "true" : "false",
        string str    => stringify(str),
        _             => stringify(value.ToString())
    };

    private static string stringify(string raw) => '"' + HttpUtility.JavaScriptStringEncode(raw) + '"';

}