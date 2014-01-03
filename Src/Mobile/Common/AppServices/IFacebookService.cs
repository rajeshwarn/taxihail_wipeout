using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IFacebookService
    {
		void Connect(string permissions);

		Task<FacebookUserInfo> GetUserInfo(string accessToken);
    }
}

