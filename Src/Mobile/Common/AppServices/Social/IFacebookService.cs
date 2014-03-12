using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public interface IFacebookService
    {
		Task Connect();
		void Disconnect();
		Task<FacebookUserInfo> GetUserInfo();
    }
}

