using System.Globalization;

namespace apcurium.MK.Common.Helpers
{
    public static class PhoneHelper
    {
        public static bool IsPossibleNumber(CountryCode countryCode, string phoneNumber)
        {
            return countryCode.IsValid() && libphonenumber.PhoneNumberUtil.Instance.IsPossibleNumber(phoneNumber, countryCode.CountryISOCode.Code);
        }

        public static string GetDigitsFromPhoneNumber(string phoneNumber)
        {
            string digits = "";
            foreach (char c in phoneNumber)
            {
                if (IsDigit(c))
                {
                    digits += c;
                }
            }

            return digits;
        }

        private static bool IsLatin1(char ch)
        {
            return (ch <= '\x00ff');
        }

        private static bool IsDigit(char c)
        {
            if (IsLatin1(c))
            {
                return (c >= '0' && c <= '9');
            }
            return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber);
        }
    }
}