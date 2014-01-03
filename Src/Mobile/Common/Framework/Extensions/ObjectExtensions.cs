using System;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class ObjectExtensions
    {
        public static ExtensionPoint<T> Extensions<T>(this T value)
        {
            return new ExtensionPoint<T>(value);
        }

        public static bool IsDefault<T>(this ExtensionPoint<T> extensionPoint)
        {
            return Equals(extensionPoint.ExtendedValue, default(T));
        }

        public static void Dispose<T>(this ExtensionPoint<T> extensionPoint)
        {
            var disposable = extensionPoint.ExtendedValue as IDisposable;

            disposable.Maybe(d => d.Dispose());
        }

        public static IDisposable Using<T>(this ExtensionPoint<T> extensionPoint)
        {
            var disposable = extensionPoint.ExtendedValue as IDisposable;

            return disposable ?? NullDisposable.Instance;
        }

        public static void Maybe<TInstance>(this TInstance instance, Action action)
        {
            if (instance != null)
            {
                action();
            }
        }

        public static void Maybe<TInstance>(this TInstance instance, Action<TInstance> action)
        {
            if (instance != null)
            {
                action(instance);
            }
        }

        public static void Maybe<TInstance>(this object instance, Action<TInstance> action) where TInstance : class
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
            return instance == null ? defaultValue : selector(instance);
        }

        public static string ToSafeString<TInstance>(this TInstance instance)
        {
            if (instance != null)
            {
                return instance.ToString();
            }
            return "";
        }
    }
}