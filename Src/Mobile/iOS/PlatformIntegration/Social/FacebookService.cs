using System;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using Foundation;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using Facebook.LoginKit;
using Facebook.CoreKit;
using UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social
{
	public class FacebookService: IFacebookService
    {
        // seconds
        private static readonly double MinimumTimeTokenStayActive = 60 * 60;

        private static string _facebookApplicationID;

        public static void UIApplicationDelegateFinishedLaunching(UIApplication app, NSDictionary options)
        {
            ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);
        }

        public static void UIApplicationDelegateOnActivated()
        {
            AppEvents.ActivateApp();
        }

        public static bool UIApplicationDelegateOpenURL(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation, string applicationName)
        {
            Facebook.CoreKit.Settings.AppID = _facebookApplicationID;
            Facebook.CoreKit.Settings.DisplayName = applicationName;
            return Facebook.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        public static string FacebookApplicationID
        {
            get
            {
                return _facebookApplicationID;
            }
        }

		public void Init()
		{
            try
            {
                _facebookApplicationID = Mvx.Resolve<apcurium.MK.Common.Configuration.IAppSettings>().Data.FacebookAppId;

                Facebook.CoreKit.Settings.AppID = _facebookApplicationID;
                Facebook.CoreKit.Settings.AppUrlSchemeSuffix = Mvx.Resolve<IAppSettings>().Data.TaxiHail.ApplicationName.ToLower().Replace (" ", string.Empty);
                Profile.EnableUpdatesOnAccessTokenChange(true);

                if (AccessToken.CurrentAccessToken == null
                    || (AccessToken.CurrentAccessToken != null
						&& NSDate.Now.SecondsSinceReferenceDate >= AccessToken.CurrentAccessToken.ExpirationDate.AddSeconds(-MinimumTimeTokenStayActive).SecondsSinceReferenceDate))
                {
                    LoginManager loginManager = new LoginManager();
                    loginManager.LoginBehavior = LoginBehavior.Native;
                    loginManager.LogInWithReadPermissions(new string[] { "public_profile", "email" }, (LoginManagerLoginResult result, NSError error) => {});
                }
            }
            catch(Exception ex)
            {
                Logger.LogMessage("Facebook Init failed");
                Logger.LogError(ex);
            }
		}

		public void PublishInstall()
		{
		}

        public System.Threading.Tasks.Task Connect()
		{
            var tcs = new TaskCompletionSource<object>();

            try
            {
                if (AccessToken.CurrentAccessToken == null
                    || (AccessToken.CurrentAccessToken != null
						&& NSDate.Now.SecondsSinceReferenceDate >= AccessToken.CurrentAccessToken.ExpirationDate.AddSeconds(-MinimumTimeTokenStayActive).SecondsSinceReferenceDate))
                {
                    LoginManager loginManager = new LoginManager();
                    loginManager.LoginBehavior = LoginBehavior.Native;
                    loginManager.LogInWithReadPermissions(new string[] { "public_profile", "email" }, (LoginManagerLoginResult result, NSError error) =>
                        {
                            if (error == null)
                            {
                                tcs.TrySetResult(result);
                            }
                            else
                            {
                                tcs.TrySetException(new Exception(error.ToString()));
                            }
                        });
                }
                else
                {
                    tcs.SetResult(new LoginManagerLoginResult(AccessToken.CurrentAccessToken, false, AccessToken.CurrentAccessToken.Permissions, new NSSet()));
                }
            }
            catch(Exception e)
            {
                tcs.TrySetException(e);
            }

            return tcs.Task;
		}

		public void Disconnect()
		{
            LoginManager loginManager = new LoginManager();
            loginManager.LogOut();
		}

		public Task<FacebookUserInfo> GetUserInfo()
		{
            var keys = new object [] { "fields" };
            var values = new object [] { "id,first_name,last_name,email" };

            string version = new GraphRequest("/me", null).Version;
            string httpMethod = new GraphRequest("/me", null).HTTPMethod;

            GraphRequest graphRequest = new GraphRequest("/me", NSDictionary.FromObjectsAndKeys(values, keys), AccessToken.CurrentAccessToken.TokenString, version, httpMethod);

            var tcs = new TaskCompletionSource<FacebookUserInfo>();

            graphRequest.Start((GraphRequestConnection connection, NSObject result, NSError error) =>
                {
                    if(error == null)
                    {
                        tcs.TrySetResult(FacebookUserInfo.CreateFrom(((NSDictionary)result).Select(v => new KeyValuePair<string, string>(v.Key.ToString(), v.Value.ToString())).ToDictionary(v => v.Key, v => v.Value)));
                    }
                    else
                    {
                        tcs.SetException(new NSErrorException(error));
                    }
                });

			return tcs.Task;
		}
    }
}