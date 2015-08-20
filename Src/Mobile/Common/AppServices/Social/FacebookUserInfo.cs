using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public partial class FacebookUserInfo
    {
		public string Id { get; private set; }
		public string Firstname { get; private set; }
		public string Lastname { get; private set; }
		public string Email { get; private set; }

		public static FacebookUserInfo CreateFrom(IDictionary<string, string> data)
		{
			return new FacebookUserInfo
			{
				Id = data["id"],
				Email = data["email"],
				Firstname = data["first_name"],
				Lastname = data["last_name"],
			};
		}
    }
}