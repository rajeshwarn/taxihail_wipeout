#region

using System;

#endregion

namespace apcurium.MK.Common.Entity
{
    [Flags]
    public enum DayOfTheWeek
    {
        None = 0,
        Sunday = 0x1,
        Monday = 0x2,
        Tuesday = 0x4,
        Wednesday = 0x8,
        Thursday = 0x10,
        Friday = 0x20,
        Saturday = 0x40,
    }
}