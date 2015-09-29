using System.Linq;
using System.Text.RegularExpressions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Helpers
{
	public static class EmailHelper
	{
		private const string EmailValidator = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		private static readonly Regex _emailValidator = new Regex(EmailValidator);

		public static bool IsEmail(string email)
		{
			if (email.HasValue() && _emailValidator.IsMatch(email))
			{
				return true;
			}

			return false;
		}
	}
}