using System;

namespace apcurium.MK.Common
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; } 
    }

    public class SystemClock : IClock
    {
        public DateTime Now { get { return DateTime.Now; } }
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}