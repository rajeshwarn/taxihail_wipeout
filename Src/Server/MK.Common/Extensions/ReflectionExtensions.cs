using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace apcurium.MK.Common.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Extension method that returns all the properties from a class, including
        /// the properties from its nested and parent class, with their fully qualified name.
        /// </summary>
        /// <param name="type">The object type on which to get the property of.</param>
        /// <returns>Dictionnary containing all the properties from a class.
        /// Key: Fully qualified property name
        /// Value: The PropertyInfo object</returns>
        public static IDictionary<string, PropertyInfo> GetAllProperties(this Type type)
        {
            var allProperties = new Dictionary<string, PropertyInfo>();

            type.GetNestedTypeProperties(string.Empty, allProperties);

            return allProperties;
        }

        private static void GetNestedTypeProperties(this Type type, string fullName, IDictionary<string, PropertyInfo> allProperties)
        {
            // Find complex properties
            var nestedTypes = (from typeProperty in type.GetProperties()
                               where typeProperty.PropertyType.IsUserDefinedClass()
                               select typeProperty.PropertyType).ToList();

            // Add normal properties
            var nonNestedPropertyTypes = type.GetProperties().Where(x => !nestedTypes.Contains(x.PropertyType));
            foreach (var nonNestedPropertyType in nonNestedPropertyTypes)
            {
                string fullyQualifiedName = fullName.Length > 0 
                    ? string.Format("{0}.{1}", fullName, nonNestedPropertyType.Name)
                    : nonNestedPropertyType.Name;

                allProperties.Add(fullyQualifiedName, nonNestedPropertyType);
            }

            if (nestedTypes.Any())
            {
                // Recursively loop through all the nested properties
                var nestedPropertyTypes = type.GetProperties().Where(x => nestedTypes.Contains(x.PropertyType));
                foreach (var nestedProperty in nestedPropertyTypes)
                {
                    string fullyQualifiedNestedName = fullName.Length > 0
                        ? string.Format("{0}.{1}", fullName, nestedProperty.Name)
                        : nestedProperty.Name;

                    nestedProperty.PropertyType.GetNestedTypeProperties(fullyQualifiedNestedName, allProperties);
                }
            }
        }

        /// <summary>
        /// Extension method that returns the value of a nested property of a class.
        /// </summary>
        /// <param name="obj">The object on which to get the property value from.</param>
        /// <param name="fullyQualifiedName">The fully qualified name of the property to get the value of.</param>
        /// <returns>The property value.</returns>
        public static object GetNestedPropertyValue(this object obj, string fullyQualifiedName)
        {
            foreach (string part in fullyQualifiedName.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        /// <summary>
        /// Extension method that checks if the type is user defined.
        /// </summary>
        /// <param name="type">The type to check if it is user defined.</param>
        /// <returns>True if the type is user defined; false otherwise.</returns>
        public static bool IsUserDefinedClass(this Type type)
        {
            return type.IsClass
                   && !type.IsPrimitive
                   && !type.IsEnum
                   && !type.FullName.StartsWith("System.");
        }
    }
}
