using System;
using System.Threading.Tasks;
using Android.App;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using Xamarin.FacebookBinding;
using Xamarin.FacebookBinding.Model;
using Android.Content.PM;
using Java.Security;
using Android.Util;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid.Platform;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Services.Social
{
	public class FacebookService: IFacebookService
	{
		private readonly string _appId;
        private readonly Func<Activity> _mainActivity;
        private readonly MyStatusCallback _statusCallback;

		public FacebookService()
		{
			this._mainActivity = () => TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
			this._appId = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().Data.FacebookAppId;
			this._statusCallback = new MyStatusCallback();
		}

		public void Init()
		{

		}

		public void PublishInstall()
		{
            try
            {
                Xamarin.FacebookBinding.Settings.PublishInstallAsync(TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity.ApplicationContext , _appId );
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
					//Ugly hack until we upgrade the facebook sdk.
					if (IsFacebookInstalled ()) 
                    { 
						openRequest.SetPermissions (new [] { "basic_info", "email" });
					} else 
                    {
						openRequest.SetPermissions (new [] { "public_profile", "email"  });
					}
					openRequest.SetLoginBehavior(SessionLoginBehavior.SsoWithFallback);
					openRequest.SetDefaultAudience (SessionDefaultAudience.Friends);

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

		private bool IsFacebookInstalled()
		{
			try
            {
				var dataUri = Android.Net.Uri.Parse("fb://....");
				var  receiverIntent = new Intent(Intent.ActionView, dataUri);
				var packageManager =  TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity.PackageManager;
				var activities = packageManager.QueryIntentActivities(receiverIntent, (PackageInfoFlags) 0);

				return activities.Count  > 0;
			}
			catch
			{
				return false;
			}
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

		private class MyStatusCallback : Java.Lang.Object, Session.IStatusCallback
		{
			readonly object _gate = new object();
			private TaskCompletionSource<object> _tcs;
			private Session _currentTaskSession;

			public MyStatusCallback ()
			{
			}

			public void Call (Session session, SessionState status, Java.Lang.Exception exception)
			{
				bool connected = status == SessionState.Opened
					|| status == SessionState.OpenedTokenUpdated;


				if (!connected && ( _currentTaskSession == null || _currentTaskSession != session))
				{
					return;
				}

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

		private class MyGraphUserCallback: Java.Lang.Object, Request.IGraphUserCallback
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

