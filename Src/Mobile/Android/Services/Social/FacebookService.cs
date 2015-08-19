using System;
using System.Threading.Tasks;
using Android.App;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using Android.Content.PM;
using Java.Security;
using Android.Util;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid.Platform;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Android.OS;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Xamarin.Facebook.AppEvents;

namespace apcurium.MK.Booking.Mobile.Client.Services.Social
{
	public class FacebookService : IFacebookService
	{
		// milliseconds
		private static readonly long MinimumTimeTokenStayActive = 60 * 60 * 1000; 
		
		private static string _facebookApplicationID;
		ICallbackManager _facebookCallbackManager;
		FacebookCallback<Java.Lang.Object> _facebookCallback = new FacebookCallback<Java.Lang.Object>();

		public void ActivityOnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			_facebookCallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
		}

		public void Init()
		{
			if (!FacebookSdk.IsInitialized)
			{
				_facebookApplicationID = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().Data.FacebookAppId;

				FacebookSdk.ApplicationId = _facebookApplicationID;
				FacebookSdk.ApplicationName = Mvx.Resolve<IAppSettings>().Data.TaxiHail.ApplicationName.ToLower().Replace(" ", string.Empty);
				FacebookSdk.SdkInitialize(Application.Context);
				_facebookCallbackManager = CallbackManagerFactory.Create();
				LoginManager.Instance.RegisterCallback(_facebookCallbackManager, _facebookCallback);
			}
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
					Activity currentActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
					_facebookCallback.SetTaskCompletionSource(loginTaskCompletionSource);

					LoginManager.Instance.SetLoginBehavior(LoginBehavior.NativeWithFallback);
					LoginManager.Instance.LogInWithReadPermissions(currentActivity, new string[] { "public_profile", "email" });
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
			var tcs = new TaskCompletionSource<FacebookUserInfo>();

			Bundle parameters = new Bundle();
			parameters.PutString("fields", "id,first_name,last_name,email");

			GraphRequest graphRequestData = new GraphRequest();
			GraphRequest userInfoGraphRequest = new GraphRequest(AccessToken.CurrentAccessToken, "me", parameters, graphRequestData.HttpMethod, new GetUserInfoCallback(tcs), graphRequestData.Version);

			userInfoGraphRequest.ExecuteAsync();
			
			return tcs.Task;
		}

		public void PublishInstall()
		{
			AppEventsLogger.ActivateApp(Application.Context, _facebookApplicationID);
		}

		class GetUserInfoCallback : Java.Lang.Object, GraphRequest.ICallback
		{
			TaskCompletionSource<FacebookUserInfo> _tcs;

			public GetUserInfoCallback(TaskCompletionSource<FacebookUserInfo> tcs)
			{
				_tcs = tcs;
			}

			public void OnCompleted(GraphResponse response)
			{
				if (response.Error == null)
				{
					Org.Json.JSONArray userInfoKeys = response.JSONObject.Names();

					Dictionary<string, string> userInfo = new Dictionary<string, string>();

					for (int i = 0; i < userInfoKeys.Length(); i++)
					{
						userInfo.Add(userInfoKeys.Get(i).ToString(), response.JSONObject.GetString(userInfoKeys.Get(i).ToString()));
					}

					_tcs.TrySetResult(FacebookUserInfo.CreateFrom(userInfo));
				}
				else
				{
					_tcs.TrySetException(response.Error.Exception);
				}
			}
		}

		class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
		{
			static object exclusiveAccess = new object();
			private TaskCompletionSource<object> _loginTaskCompletionSource;

			public void SetTaskCompletionSource(TaskCompletionSource<object> tcs)
			{
				lock (exclusiveAccess)
				{
					if (_loginTaskCompletionSource != null)
					{
						_loginTaskCompletionSource.TrySetCanceled();
					}

					_loginTaskCompletionSource = tcs;
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