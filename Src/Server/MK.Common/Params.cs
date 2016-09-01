#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Common
{
    public static class Params
    {
        public static IEnumerable<T> Get<T>(params T[] values)
        {
            return values;
        }
    }
}