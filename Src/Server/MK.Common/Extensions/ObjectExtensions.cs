#region

using System;
using System.Collections.Generic;
using System.ComponentModel;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static void Maybe<TInstance>(this TInstance instance, Action action)
        {
            if (!Equals(instance, default(TInstance)))
            {
                action();
            }
        }

        public static void Maybe<TInstance>(this TInstance instance, Action<TInstance> action)
        {
            if (!Equals(instance, default(TInstance)))
            {
                action(instance);
            }
        }

        public static void Maybe<TInstance>(this object instance, Action<TInstance> action)
            where TInstance : class
        {
            Maybe(instance as TInstance, action);
        }

        public static TResult SelectOrDefault<TInstance, TResult>(this TInstance instance,
            Func<TInstance, TResult> selector)
        {
            return SelectOrDefault(instance, selector, default(TResult));
        }

        public static TResult SelectOrDefault<TInstance, TResult>(this TInstance instance,
            Func<TInstance, TResult> selector, TResult defaultValue)
        {
            return Equals(instance, default(TInstance)) ? defaultValue : selector(instance);
        }

        public static IDictionary<string, object> AddProperty(this object obj, string name, object value)
        {
            var dictionary = obj.ToDictionary();
            dictionary.Add(name, value);
            return dictionary;
        }

        private static IDictionary<string, object> ToDictionary(this object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }
            return result;
        }
    }
}