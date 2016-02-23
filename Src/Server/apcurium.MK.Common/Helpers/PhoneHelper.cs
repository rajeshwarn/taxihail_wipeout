using System.Linq;
using System.Text.RegularExpressions;

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
            var digits = phoneNumber.Where(char.IsDigit).ToArray();

            return new string(digits);
        }
    }
}