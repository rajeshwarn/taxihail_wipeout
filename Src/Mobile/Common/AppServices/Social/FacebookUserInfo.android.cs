using Xamarin.FacebookBinding.Model;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public partial class FacebookUserInfo
    {
		public static FacebookUserInfo CreateFrom(IGraphUser user)
		{
			return new FacebookUserInfo
			{
				Id = user.Id,
				Email = user.GetProperty("email").ToString(),
				Firstname = user.FirstName,
				Lastname = user.LastName,
			};
		}
    }
}

