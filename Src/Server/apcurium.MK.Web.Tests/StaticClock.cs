using System;
using apcurium.MK.Common;

namespace apcurium.MK.Web.Tests
{
    public class StaticClock : IClock
    {
        public DateTime Now { get { return new DateTime(2014, 12, 2, 9, 6, 13); } }
        public DateTime UtcNow { get { return new DateTime(2014, 12, 2, 9, 6, 13); } }
    }
}