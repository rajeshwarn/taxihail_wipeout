using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public interface IFacebookService
    {
		void Init();
		Task Connect();
		void Disconnect();
		void PublishInstall();
		Task<FacebookUserInfo> GetUserInfo();
    }
}

