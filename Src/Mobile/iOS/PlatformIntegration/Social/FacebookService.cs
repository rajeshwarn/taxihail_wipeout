using System;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using Foundation;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social
{
	public class FacebookService: IFacebookService
    {
        private string _appId;

		public void Init()
		{
            try
            {
                _appId = Mvx.Resolve<IAppSettings>().Data.FacebookAppId;

//                FBSettings.DefaultAppID = _appId;
//                FBSettings.DefaultUrlSchemeSuffix = Mvx.Resolve<IAppSettings>().Data.TaxiHail.ApplicationName.ToLower ().Replace (" ", string.Empty);
//
//                if (FBSession.ActiveSession.State == FBSessionState.CreatedTokenLoaded) 
//                {
//                    // If there's one, just open the session silently
//                    FBSession.OpenActiveSession (new[] { "public_profile", "email" },
//                        allowLoginUI: false,
//                        completion: (session, status, error) => {});
//                }                   
            }
            catch(Exception ex)
            {
                Logger.LogMessage("Facebook Init failed");
                Logger.LogError(ex);
            }
		}

		public void PublishInstall()
		{
            try
            {
                //FBAppEvents.ActivateApp ();
            }
            catch(Exception ex)
            {
                Logger.LogMessage("Facebook PublishInstall failed");
                Logger.LogError(ex);
            }
		}

		public Task Connect()
		{
			// If the session state is any of the two "open" states when the button is clicked
//			if (FBSession.ActiveSession.State == FBSessionState.Open
//				|| FBSession.ActiveSession.State == FBSessionState.OpenTokenExtended)
//			{
//
//				// Close the session and remove the access token from the cache
//				// The session state handler (in the app delegate) will be called automatically
//				FBSession.ActiveSession.CloseAndClearTokenInformation();
//			}

			var tcs = new TaskCompletionSource<object>();
			// Open a session showing the user the login UI
			// You must ALWAYS ask for basic_info permissions when opening a session
			try
			{
//				FBSession.OpenActiveSession(new [] {"public_profile", "email"},
//					allowLoginUI: true,
//					completion: (session, status, error) =>
//					{
//						var connected = status == FBSessionState.Open
//						                 || status == FBSessionState.OpenTokenExtended;
//
//						if(connected)
//						{
//							tcs.TrySetResult(null);
//						}
//						else if (error != null)
//						{
//							tcs.TrySetException(new NSErrorException(error));
//						}
//
//					});
			}
			catch(Exception e)
			{
				tcs.TrySetException(e);
			}

			return tcs.Task;
		}

		public void Disconnect()
		{
			// If the session state is any of the two "open" states when the button is clicked
//			if (FBSession.ActiveSession.State == FBSessionState.Open
//				|| FBSession.ActiveSession.State == FBSessionState.OpenTokenExtended)
//			{
//				// Close the session and remove the access token from the cache
//				// The session state handler (in the app delegate) will be called automatically
//				FBSession.ActiveSession.CloseAndClearTokenInformation();
//			}
		}

		public Task<FacebookUserInfo> GetUserInfo()
		{
			var tcs = new TaskCompletionSource<FacebookUserInfo>();
//			FBRequestConnection.GetMe((connection, result, error) =>
//			{
//				if(error == null)
//				{
//					var graph = (FBGraphObject)result;
//					tcs.TrySetResult(FacebookUserInfo.CreateFrom(graph));
//				}
//				else
//				{
//					tcs.SetException(new NSErrorException(error));
//				}
//			});
			return tcs.Task;
		}
    }
}

