using System;

namespace apcurium.Framework.Extensions
{
    public static class ActionExtensions
    {
        public static IDisposable ToDisposable(this Action action)
        {
            return new DisposableAction(action);
        }
    }
}
