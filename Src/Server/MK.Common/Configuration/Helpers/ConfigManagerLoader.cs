using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;

namespace apcurium.MK.Common.Configuration.Helpers
{
    public static class ConfigManagerLoader
    {
        public static void InitializeDataObjects<T>(T objectToInitialize, IDictionary<string, string> values, ILogger logger) where T : class
        {
            var typeOfSettings = typeof(T);
            foreach (KeyValuePair<string, string> item in values)
            {
                try
                {
                    var propertyName = item.Key;
                    if (propertyName.Contains("."))
                    {
                        if (propertyName.SplitOnFirst('.')[0] == "Client")
                        {
                            propertyName = propertyName.SplitOnFirst('.')[1];
                        }
                    }

                    var propertyType = GetProperty(typeOfSettings, propertyName);

                    if (propertyType == null)
                    {
                        Console.WriteLine("Warning - can't set value for property {0}, value was {1} - property not found", propertyName, item.Value);
                        continue;
                    }

                    var targetType = IsNullableType(propertyType.PropertyType)
                        ? Nullable.GetUnderlyingType(propertyType.PropertyType)
                        : propertyType.PropertyType;

                    if (targetType.IsEnum)
                    {
                        var propertyVal = Enum.Parse(targetType, item.Value);
                        SetValue(propertyName, objectToInitialize, propertyVal);
                    }
                    else if (IsNullableType(propertyType.PropertyType) && string.IsNullOrEmpty(item.Value))
                    {
                        SetValue(propertyName, objectToInitialize, null);
                    }
                    else
                    {
                        var propertyVal = Convert.ChangeType(item.Value, targetType);
                        SetValue(propertyName, objectToInitialize, propertyVal);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value);
                    logger.Maybe(() => logger.LogMessage("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value));
//                    _logger.Maybe(() => _logger.LogError(e));
                }
            }
        }

        private static void SetValue(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            var propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

        private static PropertyInfo GetProperty(Type type, string propertyName)
        {
            if (type.GetProperties().Count(p => p.Name == propertyName.Split('.')[0]) == 0)
            {
                throw new ArgumentNullException(string.Format("Property {0}, does not exist in type {1}", propertyName, type));
            }

            if (propertyName.Split('.').Length == 1)
            {
                return type.GetProperty(propertyName);
            }

            var firstProperty = type.GetProperty(propertyName.Split('.')[0]);
            if (firstProperty == null)
            {
                return null;
            }
            return GetProperty(firstProperty.PropertyType, propertyName.Split('.')[1]);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}