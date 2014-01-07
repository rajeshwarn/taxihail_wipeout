using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IFacebookService
    {
		Task Connect(string permissions);
		void Disconnect();
		Task<FacebookUserInfo> GetUserInfo();
    }
}

