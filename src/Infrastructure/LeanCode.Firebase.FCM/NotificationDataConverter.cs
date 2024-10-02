using System.Collections.Immutable;
using System.Reflection;

namespace LeanCode.Firebase.FCM;

public sealed class NotificationDataConverter
{
    private const string TypeField = "Type";

    private ImmutableDictionary<Type, Func<object, string>> formatters = ImmutableDictionary<
        Type,
        Func<object, string>
    >.Empty;

    public void FormatUsing<T>(Func<T, string> formatter)
    {
        formatters = formatters.Add(typeof(T), o => formatter((T)o));
    }

    /// <summary>
    /// Converts POCO object to a notification data dictionary. Does not support hierarchical
    /// data.
    /// </summary>
    public Dictionary<string, string> ToNotificationData(object data)
    {
        var type = data.GetType();

        var result = new Dictionary<string, string>() { [TypeField] = type.Name, };

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name != TypeField)
            {
                var value = prop.GetValue(data);

                if (value is not null)
                {
                    var formatter = formatters.GetValueOrDefault(value.GetType(), DefaultValueFormatter);
                    result.Add(prop.Name, formatter(value));
                }
            }
        }

        return result;
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
}
