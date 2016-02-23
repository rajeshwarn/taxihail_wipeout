using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using System.Globalization;

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
                    var propertyName = overriddenSetting.Key;
                    try
					{
						var propertyType = GetProperty(typeOfSettings, propertyName);

						if (propertyType == null)
						{
#if DEBUG
                            Debug.WriteLine("Warning - can't set value for property {0}, value was {1} - property not found", propertyName, overriddenSetting.Value);
#endif
							continue;
						}

						var targetType = IsNullableType(propertyType.PropertyType)
							? Nullable.GetUnderlyingType(propertyType.PropertyType)
							: propertyType.PropertyType;

						if (targetType.GetTypeInfo().IsEnum)
						{
							var propertyVal = Enum.Parse(targetType, overriddenSetting.Value, true);
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
#if DEBUG
                                Debug.WriteLine("Warning - can't set value for property {0}, value was {1}", overriddenSetting.Key, overriddenSetting.Value);
#endif
						    }
                            else
                            {
                                var propertyVal = Convert.ChangeType(overriddenSetting.Value, targetType, CultureInfo.InvariantCulture);
                                SetValue(propertyName, objectToInitialize, propertyVal);
                            }
						}
					}
					catch (Exception e)
					{
#if DEBUG
                        Debug.WriteLine("Warning - can't set value for property {0}, value was {1}", propertyName, overriddenSetting.Value);
                        logger.Maybe(() => logger.LogMessage("Warning - can't set value for property {0}, value was {1}", propertyName, overriddenSetting.Value));
#endif
                        logger.Maybe(() => logger.LogError(e));
					}
				}
            }
        }

        private static void SetValue(string compoundProperty, object target, object value)
        {
            var propertyPath = compoundProperty.Split('.');
            for (var i = 0; i < propertyPath.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetRuntimeProperty(propertyPath[i]);
                target = propertyToGet.GetValue(target, null);
            }
            var propertyToSet = target.GetType().GetRuntimeProperty(propertyPath.Last());

            //Property does not have a setter.
            if (!propertyToSet.CanWrite)
            {
                return;
            }

            propertyToSet.SetValue(target, value, null);
        }

        private static PropertyInfo GetProperty(Type type, string propertyName)
        {
            if (type.GetRuntimeProperties().Count(p => p.Name == propertyName.Split('.')[0]) == 0)
            {
                return null;
            }

            if (propertyName.Split('.').Length == 1)
            {
                return type.GetRuntimeProperty(propertyName);
            }

            var firstProperty = type.GetRuntimeProperty(propertyName.SplitOnFirst(".")[0]);
            if (firstProperty == null)
            {
                return null;
            }
            return GetProperty(firstProperty.PropertyType, propertyName.SplitOnFirst(".")[1]);
        }

        private static bool IsNullableType(Type type)
        {
            return type.GetTypeInfo().IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}