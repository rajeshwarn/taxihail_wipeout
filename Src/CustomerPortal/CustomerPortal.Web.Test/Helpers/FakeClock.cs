#region

using System;
using CustomerPortal.Web.Services;

#endregion

namespace CustomerPortal.Web.Test.Helpers
{
    public class FakeClock : IClock
    {
        private readonly DateTime _utcNow;

        public FakeClock(DateTime utcNow)
        {
            _utcNow = utcNow;
        }

        public DateTime UtcNow
        {
            get { return _utcNow; }
        }
    }
}