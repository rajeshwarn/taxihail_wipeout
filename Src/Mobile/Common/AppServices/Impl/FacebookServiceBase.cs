using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public abstract class FacebookServiceBase: IFacebookService
    {
		public abstract void Connect(string permissions);

		public async Task<FacebookUserInfo> GetUserInfo(string accessToken)
		{
			var fb = new FacebookClient(accessToken);
			var me = fb.GetTaskAsync("me");
			return FacebookUserInfo.CreateFrom((IDictionary<string, object>) await me);
		}

    }
}

