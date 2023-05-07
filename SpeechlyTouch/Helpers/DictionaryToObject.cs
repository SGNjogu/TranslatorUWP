using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeechlyTouch.Helpers
{
    public static class DictionaryToObject
    {
        /// <summary>
        /// Convert a dictionary of strings to a an object
        /// Uses reflection to match dictionary keys to object properties
        /// Make sure the key names are equal to the property names in T
        /// </summary>
        /// <typeparam name="T">Type of resulting object</typeparam>
        /// <param name="dict">Key value pairs</param>
        /// <returns>T</returns>
        public static T Convert<T>(IDictionary<string, string> dict) where T : new()
        {
            var t = new T();
            PropertyInfo[] properties = t.GetType().GetProperties();
            Func<KeyValuePair<string, string>, PropertyInfo, bool> func = (kv, property) => kv.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase);

            foreach (PropertyInfo property in properties)
            {
                if (!dict.Any(x => func(x, property)))
                    continue;

                KeyValuePair<string, string> item = dict.First(x => func(x, property));

                // Find which type the current property is
                Type tPropertyType = property.PropertyType;

                // Fix nullables
                Type newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;
                object newA = System.Convert.ChangeType(item.Value, newT);

                property.SetValue(t, newA, null);
            }

            return t;
        }
    }
}
