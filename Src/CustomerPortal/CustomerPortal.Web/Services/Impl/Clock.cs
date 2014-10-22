#region

using System;

#endregion

namespace CustomerPortal.Web.Services.Impl
{
    public class Clock : IClock
    {
        public static readonly IClock Instance = new Clock();

        private Clock()
        {
        }

        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}