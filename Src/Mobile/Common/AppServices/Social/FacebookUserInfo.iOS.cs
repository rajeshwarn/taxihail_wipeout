using System;
using MonoTouch.FacebookConnect;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public partial class FacebookUserInfo
    {
		public static FacebookUserInfo CreateFrom(FBGraphObject data)
		{
			return new FacebookUserInfo
			{
				Id = (NSString)data["id"],
				Email = (NSString)data["email"],
				Firstname = (NSString)data["first_name"],
				Lastname = (NSString)data["last_name"],
			};
		}
    }
}

