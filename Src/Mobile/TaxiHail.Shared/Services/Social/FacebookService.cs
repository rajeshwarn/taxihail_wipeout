using System.Threading.Tasks;
using Android.App;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid.Platform;
using Android.Content;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Android.OS;
using System.Collections.Generic;
using Xamarin.Facebook.AppEvents;

namespace apcurium.MK.Booking.Mobile.Client.Services.Social
{
	public class FacebookService : IFacebookService
	{
		// milliseconds
	    private const long MinimumTimeTokenStayActive = 60*60*1000;

	    private static string _facebookApplicationId;
	    private ICallbackManager _facebookCallbackManager;
	    private readonly FacebookCallback<Java.Lang.Object> _facebookCallback = new FacebookCallback<Java.Lang.Object>();

		public void ActivityOnActivityResult(int requestCode, Result resultCode, Intent data)
		{
		    if (!FacebookSdk.IsFacebookRequestCode(requestCode))
		    {
		        return;
		    }
			_facebookCallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
		}

		public void Init()
		{
		    if (FacebookSdk.IsInitialized)
		    {
		        return;
		    }
		    var appSettings = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>();

		    _facebookApplicationId = appSettings.Data.FacebookAppId;
		    FacebookSdk.ApplicationId = _facebookApplicationId;
		    FacebookSdk.ApplicationName = appSettings.Data.TaxiHail.ApplicationName.ToLower().Replace(" ", string.Empty);
		    FacebookSdk.SdkInitialize(Application.Context);
		    _facebookCallbackManager = CallbackManagerFactory.Create();
		    LoginManager.Instance.RegisterCallback(_facebookCallbackManager, _facebookCallback);
		}

		/// <summary>
		/// For ANDROID: you have to implement override void OnActivityResult(int requestCode, Result resultCode, Intent data) in the Activity class where you call this method
		/// and call FacebookService.ActivityOnActivityResult(requestCode, resultCode, data) inside OnActivityResult
		/// </summary>
		public Task Connect()
		{
			var loginTaskCompletionSource = new TaskCompletionSource<object>();

			if (FacebookSdk.IsInitialized)
			{
				if (AccessToken.CurrentAccessToken == null
						|| (AccessToken.CurrentAccessToken != null
							&& (new Java.Util.Date()).CompareTo(new Java.Util.Date(AccessToken.CurrentAccessToken.Expires.Time - MinimumTimeTokenStayActive)) > 0))
				{
					var currentActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
					_facebookCallback.SetTaskCompletionSource(loginTaskCompletionSource);

					LoginManager.Instance.SetLoginBehavior(LoginBehavior.NativeWithFallback);
					LoginManager.Instance.LogInWithReadPermissions(currentActivity, new[] { "public_profile", "email" });
				}
				else
				{
					loginTaskCompletionSource.TrySetResult(new LoginResult(AccessToken.CurrentAccessToken, AccessToken.CurrentAccessToken.Permissions, AccessToken.CurrentAccessToken.DeclinedPermissions));
				}
			}
			else
			{
				loginTaskCompletionSource.TrySetException(new FacebookException("SDK not initialized"));
			}

			return loginTaskCompletionSource.Task;
		}

		public void Disconnect()
		{
			LoginManager.Instance.LogOut();
		}

		public Task<FacebookUserInfo> GetUserInfo()
		{
			var taskCompletionSource = new TaskCompletionSource<FacebookUserInfo>();

			var parameters = new Bundle();
			parameters.PutString("fields", "id,first_name,last_name,email");

			var graphRequestData = new GraphRequest();
			var userInfoGraphRequest = new GraphRequest(AccessToken.CurrentAccessToken, "me", parameters, graphRequestData.HttpMethod, new GetUserInfoCallback(taskCompletionSource), graphRequestData.Version);

			userInfoGraphRequest.ExecuteAsync();
			
			return taskCompletionSource.Task;
		}

		public void PublishInstall()
		{
			AppEventsLogger.ActivateApp(Application.Context, _facebookApplicationId);
		}

	    private class GetUserInfoCallback : Java.Lang.Object, GraphRequest.ICallback
		{
	        private readonly TaskCompletionSource<FacebookUserInfo> _taskCompletionSource;

			public GetUserInfoCallback(TaskCompletionSource<FacebookUserInfo> taskCompletionSource)
			{
				_taskCompletionSource = taskCompletionSource;
			}

			public void OnCompleted(GraphResponse response)
			{
				if (response.Error == null)
				{
					var userInfoKeys = response.JSONObject.Names();

					var userInfo = new Dictionary<string, string>();

					for (var i = 0; i < userInfoKeys.Length(); i++)
					{
						userInfo.Add(userInfoKeys.Get(i).ToString(), response.JSONObject.GetString(userInfoKeys.Get(i).ToString()));
					}

					_taskCompletionSource.TrySetResult(FacebookUserInfo.CreateFrom(userInfo));
				}
				else
				{
					_taskCompletionSource.TrySetException(response.Error.Exception);
				}
			}
		}

	    private class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
		{
			private static readonly object _exclusiveAccess = new object();
			private TaskCompletionSource<object> _loginTaskCompletionSource;

			public void SetTaskCompletionSource(TaskCompletionSource<object> taskCompletionSource)
			{
				lock (_exclusiveAccess)
				{
					if (_loginTaskCompletionSource != null)
					{
						_loginTaskCompletionSource.TrySetCanceled();
					}

					_loginTaskCompletionSource = taskCompletionSource;
				}
			}

			public void OnCancel()
			{
				if (_loginTaskCompletionSource != null)
				{
					_loginTaskCompletionSource.TrySetCanceled();
				}
			}

			public void OnError(FacebookException error)
			{
				if (_loginTaskCompletionSource != null)
				{
					_loginTaskCompletionSource.TrySetException(error);
				}
			}

			public void OnSuccess(Java.Lang.Object result)
			{
				if (_loginTaskCompletionSource != null)
				{
					_loginTaskCompletionSource.TrySetResult(result);
				}
			}
		}
	}
}