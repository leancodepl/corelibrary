using System;
using System.Collections.Generic;
using System.Reflection;

namespace LeanCode.Firebase.FCM;

public static class Notifications
{
    private const string TypeField = "Type";

    /// <summary>
    /// Converts POCO object to a notification data dictionary. Does not support hierarchical
    /// data.
    /// </summary>
    public static Dictionary<string, string> ToNotificationData(object data)
    {
        var type = data.GetType();

        var result = new Dictionary<string, string>()
        {
            [TypeField] = type.Name,
        };

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name != TypeField)
            {
                var value = prop.GetValue(data);

                if (value is not null)
                {
                    if (value is Enum @enum)
                    {
                        var strValue = Convert.ToInt32(@enum, System.Globalization.CultureInfo.InvariantCulture)
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                        result.Add(prop.Name, strValue);
                    }
                    else
                    {
                        result.Add(prop.Name, value.ToString()!);
                    }
                }
            }
        }

        return result;
    }
}
