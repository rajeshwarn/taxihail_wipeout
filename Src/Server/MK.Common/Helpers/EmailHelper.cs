using System.Linq;
using System.Text.RegularExpressions;

namespace apcurium.MK.Common.Helpers
{
	public static class EmailHelper
	{
		const string EmailValidatorTemplate = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		static Regex _emailValidator = new Regex(EmailValidatorTemplate);

		public static bool IsEmail(string email)
		{
			if (!string.IsNullOrWhiteSpace(email) && _emailValidator.IsMatch(email))
			{
				return true;
			}

			return false;
		}
	}
}