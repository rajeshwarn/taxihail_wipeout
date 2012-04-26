using System.Collections.Generic;

namespace apcurium.Framework.Extensions
{
    public static class StackExtensions
    {
        public static T PeekOrDefault<T>(this Stack<T> stack)
        {
            return stack.Empty() ? default(T) : stack.Peek();
        }

      
    }
}