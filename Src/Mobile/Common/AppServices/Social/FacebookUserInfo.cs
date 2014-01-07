using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public partial class FacebookUserInfo
    {
		public string Id { get; private set; }
		public string Firstname { get; private set; }
		public string Lastname { get; private set; }
		public string Email { get; private set; }

		public static FacebookUserInfo CreateFrom(IDictionary<string, object> data)
		{
			return new FacebookUserInfo
			{
				Id = (string)data["id"],
				Email = (string)data["email"],
				Firstname = (string)data["first_name"],
				Lastname = (string)data["last_name"],
			};
		}
    }
}

