using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Xamarin.FacebookBinding;
using Xamarin.FacebookBinding.Model;
using apcurium.MK.Booking.Mobile.AppServices.Social;

namespace apcurium.MK.Booking.Mobile.Client.Services.Social
{
	public class FacebookService: IFacebookService
	{
		readonly string _appId;
		readonly Func<Activity> _mainActivity;
		readonly MyStatusCallback _statusCallback;

		public FacebookService(string appId, Func<Activity> mainActivity)
		{
			this._mainActivity = mainActivity;
			this._appId = appId;
			this._statusCallback = new MyStatusCallback();
		}

		public Task Connect(string permissions)
		{
			// If the session state is any of the two "open" states when the button is clicked
			if (Session.ActiveSession != null 
				&& (Session.ActiveSession.State == SessionState.Opened
					|| Session.ActiveSession.State == SessionState.OpenedTokenUpdated))
			{

				// Close the session and remove the access token from the cache
				// The session state handler (in the app delegate) will be called automatically
				Session.ActiveSession.CloseAndClearTokenInformation();
			}

			// Open a session showing the user the login UI
			// You must ALWAYS ask for basic_info permissions when opening a session
			Session session = new Session.Builder(_mainActivity()).SetApplicationId(_appId).Build();
			Session.ActiveSession = session;

			var tcs = new TaskCompletionSource<object>();
			if (!session.IsOpened)
			{
				_statusCallback.SetTaskCompletionSource(tcs, session);
				Session.OpenRequest openRequest = null;

				openRequest = new Session.OpenRequest(_mainActivity());

				if (openRequest != null)
				{
					openRequest.SetPermissions(new [] { "basic_info", "email" });
					openRequest.SetLoginBehavior(SessionLoginBehavior.SsoWithFallback);

					session.OpenForRead(openRequest);
				}
				else
				{
					tcs.SetException(new Exception("Could not open request"));
				}
			}
			else
			{
				tcs.SetResult(null);
			}
			return tcs.Task;
		}

		public void Disconnect()
		{
			if (Session.ActiveSession != null
				&& (Session.ActiveSession.State == SessionState.Opened
					|| Session.ActiveSession.State == SessionState.OpenedTokenUpdated))
			{

				// Close the session and remove the access token from the cache
				Session.ActiveSession.CloseAndClearTokenInformation();
			}
		}


		public Task<FacebookUserInfo> GetUserInfo()
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
			readonly object _gate = new object();
			private TaskCompletionSource<object> _tcs;
			private Session _currentTaskSession;

			public MyStatusCallback ()
			{
			}

			public void Call (Session session, SessionState status, Java.Lang.Exception exception)
			{
				if (_currentTaskSession == null || _currentTaskSession != session)
				{
					return;
				}

				bool connected = status == SessionState.Opened
				                 || status == SessionState.OpenedTokenUpdated;

				if (_tcs != null)
				{
					if (connected)
					{
						_tcs.TrySetResult(null);
					}
					else if (exception != null)
					{
						_tcs.TrySetException(exception);
					}
				}
			}

			public void SetTaskCompletionSource(TaskCompletionSource<object> tcs, Session session)
			{
				lock (_gate)
				{
					if (_tcs != null)
					{
						_tcs.TrySetCanceled();
					}
					_tcs = tcs;
					_currentTaskSession = session;
				}
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

