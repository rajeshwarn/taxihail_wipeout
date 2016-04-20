using System.Collections.Generic;

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