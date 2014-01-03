#region

using System;

#endregion

namespace apcurium.MK.Common.Extensions
{
    public static class GuidExtensions
    {
        public static bool IsNullOrEmpty(this Guid instance)
        {
            return instance == Guid.Empty;
        }

        public static bool IsNullOrEmpty(this Guid? instance)
        {
            return instance == Guid.Empty || !instance.HasValue;
        }


        public static bool HasValue(this Guid instance)
        {
            return !instance.IsNullOrEmpty();
        }

        public static bool HasValue(this Guid? instance)
        {
            return !instance.IsNullOrEmpty();
        }
    }
}