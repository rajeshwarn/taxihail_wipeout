using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.Framework
{
    public static class Params
    {
        public static IEnumerable<T> Get<T>(params T[] values)
        {
            return values;
        }
    }
}


