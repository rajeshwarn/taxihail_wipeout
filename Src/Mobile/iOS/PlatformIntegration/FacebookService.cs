using System;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using MonoTouch.Foundation;
using System.Threading.Tasks;
using MonoTouch.FacebookConnect;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class FacebookService: FacebookServiceBase
    {

		public FacebookService()
        {

        }

		public override void Connect(string permissions)
		{
			// If the session state is any of the two "open" states when the button is clicked
			if (FBSession.ActiveSession.State == FBSessionState.Open
				|| FBSession.ActiveSession.State == FBSessionState.OpenTokenExtended)
			{

				// Close the session and remove the access token from the cache
				// The session state handler (in the app delegate) will be called automatically
				FBSession.ActiveSession.CloseAndClearTokenInformation();

				// If the session state is not any of the two "open" states when the button is clicked
			}
			else
			{
				// Open a session showing the user the login UI
				// You must ALWAYS ask for basic_info permissions when opening a session
				FBSession.OpenActiveSession(new [] {"basic_info"},
					allowLoginUI: true,
					completion: (session, status, error) =>
					{
						//var appDelegate = UIApplication.SharedApplications.Delegate;
						bool connected = status == FBSessionState.Open
						                 || status == FBSessionState.OpenTokenExtended;

						SessionStatusSubject.OnNext(connected);
					});
			}
		}

		public override Task<FacebookUserInfo> GetUserInfo(string accessToken)
		{
			throw new NotImplementedException();
		}

    }
}

