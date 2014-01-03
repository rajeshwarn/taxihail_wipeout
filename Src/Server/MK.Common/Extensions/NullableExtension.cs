namespace apcurium.MK.Common.Extensions
{
    public static class NullableExtension
    {
        public static bool IsTrueOrEmpty(this bool? val)
        {
            return !val.HasValue || val.Value;
        }

        public static bool IsTrue(this bool? val)
        {
            return val.HasValue && val.Value;
        }

        public static double ValueOrZero(this double? val)
        {
            if (val.HasValue)
            {
                return val.Value;
            }
            return 0;
        }
    }
}