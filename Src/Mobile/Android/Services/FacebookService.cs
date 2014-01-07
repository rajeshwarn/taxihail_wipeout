using System;
using Android.Content;
using Android.App;
using Facebook;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.AppServices;
using Xamarin.FacebookBinding;
using Xamarin.FacebookBinding.Model;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class FacebookService: FacebookServiceBase
    {
		readonly string _appId;
		readonly Func<Activity> _mainActivity;
		readonly FacebookClient _facebookClient = new FacebookClient();
		readonly Session.IStatusCallback _statusCallback;

		public FacebookService(string appId, Func<Activity> mainActivity)
        {
			this._mainActivity = mainActivity;
			this._appId = appId;
			this._statusCallback = new MyStatusCallback(SessionStatusObserver);
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
					openRequest.SetPermissions(new [] {"basic_info", "email"});
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

		public override Task<FacebookUserInfo> GetUserInfo()
		{
			var tcs = new TaskCompletionSource<FacebookUserInfo>();
			var currentSession = Session.ActiveSession;
			if (currentSession != null)
			{
				var request = Request.NewMeRequest(currentSession, new MyGraphUserCallback(tcs));
				Request.ExecuteBatchAsync(request);
			}
			else
			{
				throw new InvalidOperationException("No active session");
			}

			return tcs.Task;
		}

		public Session.IStatusCallback StatusCallback
		{
			get
			{
				return _statusCallback;
			}
		}

		class MyStatusCallback : Java.Lang.Object, Session.IStatusCallback
		{
			readonly IObserver<bool> _observer;
			public MyStatusCallback (IObserver<bool> observer)
			{
				this._observer = observer;
			}

			public void Call (Session session, SessionState status, Java.Lang.Exception exception)
			{
				bool connected = status == SessionState.Opened
				                 || status == SessionState.OpenedTokenUpdated;

				_observer.OnNext(connected);
			}
		}

		class MyGraphUserCallback: Java.Lang.Object, Request.IGraphUserCallback
		{
			readonly TaskCompletionSource<FacebookUserInfo> _tcs;
			public MyGraphUserCallback (TaskCompletionSource<FacebookUserInfo> tcs)
			{
				_tcs = tcs;
			}
			public void OnCompleted(IGraphUser user, Response response)
			{
				if (response.Error == null)
				{
					_tcs.TrySetResult(FacebookUserInfo.CreateFrom(user));
				}
				else
				{
					_tcs.TrySetException(response.Error.Exception);
				}
			}
		}
    }
}

