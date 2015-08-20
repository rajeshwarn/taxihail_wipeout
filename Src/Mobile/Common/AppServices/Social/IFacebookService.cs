using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public interface IFacebookService
    {
		void Init();

		/// <summary>
		/// For ANDROID: you have to implement override void OnActivityResult(int requestCode, Result resultCode, Intent data) in the Activity class where you call this method
		/// and call FacebookService.ActivityOnActivityResult(requestCode, resultCode, data) inside OnActivityResult
		/// </summary>
		Task Connect();

		void Disconnect();
	
		void PublishInstall();
		
		Task<FacebookUserInfo> GetUserInfo();
    }
}

