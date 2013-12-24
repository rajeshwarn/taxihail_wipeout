#region

using System;

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
    }
}