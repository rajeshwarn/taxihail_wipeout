using System;
using Android.Content;
using Android.App;
using Facebook;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.AppServices;
using Xamarin.FacebookBinding;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class FacebookService: FacebookServiceBase
    {
		readonly string _appId;
		readonly Func<Activity> _mainActivity;
		readonly FacebookClient _facebookClient = new FacebookClient();

		public FacebookService(string appId, Func<Activity> mainActivity)
        {
			this._mainActivity = mainActivity;
			this._appId = appId;
        }

		public override void Connect(string permissions)
		{
			// If the session state is any of the two "open" states when the button is clicked
			if (Session.ActiveSession != null 
				&& (Session.ActiveSession.State == SessionState.Opened
					|| Session.ActiveSession.State == SessionState.OpenedTokenUpdated))
			{

				// Close the session and remove the access token from the cache
				// The session state handler (in the app delegate) will be called automatically
				Session.ActiveSession.CloseAndClearTokenInformation();
				base.SessionStatusObserver.OnNext(false);
			}

			// Open a session showing the user the login UI
			// You must ALWAYS ask for basic_info permissions when opening a session
			Session session = new Session.Builder(_mainActivity()).SetApplicationId(_appId).Build();
			Session.ActiveSession = session;

			if (!session.IsOpened)
			{
				Session.OpenRequest openRequest = null;

				openRequest = new Session.OpenRequest(_mainActivity());


				if (openRequest != null)
				{
					openRequest.SetPermissions(new [] {"basic_info"});
					openRequest.SetLoginBehavior(SessionLoginBehavior.SsoWithFallback);

					session.OpenForRead(openRequest);
				}
			}


			/*Session.OpenActiveSession(new [] {"basic_info"},
				allowLoginUI: true,
				completion: (session, status, error) =>
				{
					//var appDelegate = UIApplication.SharedApplications.Delegate;
					bool connected = status == SessionState.Opened
					                 || status == SessionState.OpenedTokenUpdated;

					SessionStatusSubject.OnNext(connected);
				});*/

		}

		public async override Task<FacebookUserInfo> GetUserInfo()
		{
			/*var tcs = new TaskCompletionSource<FacebookUserInfo>();
			RequestConnection.StartForMe((connection, result, error) =>
				{
					if(error == null)
					{
						var graph = (FBGraphObject)result;
						tcs.TrySetResult(FacebookUserInfo.CreateFrom(graph));
					}
					else
					{
						tcs.SetException(new NSErrorException(error));
					}
				});
			return tcs.Task;*/

			var me = _facebookClient.GetTaskAsync("me");
			return FacebookUserInfo.CreateFrom((IDictionary<string, object>) await me);
		}

		public void SaveAccessToken(string accessToken)
		{
			this._facebookClient.AccessToken = accessToken;
			// Assume that is accessToken is no null, we are connected
			this.SessionStatusObserver.OnNext(accessToken != null);
		}
    }
}

