using System.Collections.Frozen;
using System.Reflection;

namespace LeanCode.Firebase.FCM;

public sealed class NotificationDataConverter
{
    private readonly FrozenDictionary<Type, Func<object, string>> formatters;
    private const string TypeField = "Type";

    private NotificationDataConverter(FrozenDictionary<Type, Func<object, string>> formatters)
    {
        this.formatters = formatters;
    }

    /// <summary>
    /// Converts POCO object to a notification data dictionary. Does not support hierarchical
    /// data.
    /// </summary>
    public Dictionary<string, string> ToNotificationData(object data)
    {
        var type = data.GetType();

        var result = new Dictionary<string, string>() { [TypeField] = type.Name };

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name != TypeField)
            {
                var value = prop.GetValue(data);

                if (value is not null)
                {
                    var formatter = formatters.GetValueOrDefault(GetEffectiveType(value), DefaultValueFormatter);
                    result.Add(prop.Name, formatter(value));
                }
            }
        }

        return result;
    }

    public static Builder New() => new();

    private static Type GetEffectiveType(object value)
    {
        var type = value.GetType();
        return Nullable.GetUnderlyingType(value.GetType()) ?? type;
    }

    private static string DefaultValueFormatter(object value)
    {
        if (value is Enum @enum)
        {
            return @enum.ToString("D");
        }
        else
        {
            return value.ToString()!;
        }
    }

    public class Builder
    {
        private readonly Dictionary<Type, Func<object, string>> formatters = new();

        internal Builder() { }

        public Builder AddFormatter<T>(Func<T, string> formatter)
            where T : notnull
        {
            formatters.Add(typeof(T), o => formatter((T)o));
            return this;
        }

        public NotificationDataConverter Build() => new(formatters.ToFrozenDictionary());
    }
}
