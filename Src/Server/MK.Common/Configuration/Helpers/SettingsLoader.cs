using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Configuration.Helpers
{
    public static class SettingsLoader
    {
		public static void InitializeDataObjects<T>(T objectToInitialize, IDictionary<string, string> overriddenSettings, ILogger logger, string[] excludedKeys = null) where T : class
        {
            var typeOfSettings = typeof(T);
            foreach (KeyValuePair<string, string> overriddenSetting in overriddenSettings)
            {
				if (excludedKeys == null || !excludedKeys.Any (key => overriddenSetting.Key.Contains (key))) 
				{
					try
					{
						var propertyName = overriddenSetting.Key;
						var propertyType = GetProperty(typeOfSettings, propertyName);

						if (propertyType == null)
						{
							Console.WriteLine("Warning - can't set value for property {0}, value was {1} - property not found", propertyName, overriddenSetting.Value);
							continue;
						}

						var targetType = IsNullableType(propertyType.PropertyType)
							? Nullable.GetUnderlyingType(propertyType.PropertyType)
							: propertyType.PropertyType;

						if (targetType.IsEnum)
						{
							var propertyVal = Enum.Parse(targetType, overriddenSetting.Value);
							SetValue(propertyName, objectToInitialize, propertyVal);
						}
						else if (IsNullableType(propertyType.PropertyType) && string.IsNullOrEmpty(overriddenSetting.Value))
						{
							SetValue(propertyName, objectToInitialize, null);
						}
						else
						{
                            if (targetType == typeof(bool) && string.IsNullOrEmpty(overriddenSetting.Value))
						    {
                                Console.WriteLine("Warning - can't set value for property {0}, value was {1}", overriddenSetting.Key, overriddenSetting.Value);
						    }
                            else
                            {
                                var propertyVal = Convert.ChangeType(overriddenSetting.Value, targetType);
                                SetValue(propertyName, objectToInitialize, propertyVal);
                            }
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("Warning - can't set value for property {0}, value was {1}", overriddenSetting.Key, overriddenSetting.Value);
						logger.Maybe(() => logger.LogMessage("Warning - can't set value for property {0}, value was {1}", overriddenSetting.Key, overriddenSetting.Value));
						logger.Maybe(() => logger.LogError(e));
					}
				}
            }
        }

        private static void SetValue(string compoundProperty, object target, object value)
        {
            var propertyPath = compoundProperty.Split('.');
            for (int i = 0; i < propertyPath.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetProperty(propertyPath[i]);
                target = propertyToGet.GetValue(target, null);
            }
            var propertyToSet = target.GetType().GetProperty(propertyPath.Last());
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

            var firstProperty = type.GetProperty(propertyName.SplitOnFirst(".")[0]);
            if (firstProperty == null)
            {
                return null;
            }
            return GetProperty(firstProperty.PropertyType, propertyName.SplitOnFirst(".")[1]);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        private static string[] SplitOnFirst(this string strVal, string needle)
        {
            if (strVal == null)
            {
                return new string[0];
            }

            int length = strVal.IndexOf(needle);
            if (length != -1)
            {
                return new[]
                {
                    strVal.Substring(0, length),
                    strVal.Substring(length + 1)
                };
            }

            return new[] { strVal };
        }

    }
}