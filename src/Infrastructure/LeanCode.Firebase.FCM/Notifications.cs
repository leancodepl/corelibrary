using System.Collections.Generic;
using System.Reflection;

namespace LeanCode.Firebase.FCM
{
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

                    if (value is object)
                    {
                        result.Add(prop.Name, value.ToString()!);
                    }
                }
            }

            return result;
        }
    }
}
