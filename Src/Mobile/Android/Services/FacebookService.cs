using System;
using Android.Content;
using Android.App;
using Facebook;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.AppServices;

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
			var webAuth = new Intent (_mainActivity(), typeof (FacebookWebViewAuthActivity));
			webAuth.PutExtra ("AppId", _appId);
			webAuth.PutExtra ("ExtendedPermissions", permissions);
			_mainActivity().StartActivityForResult (webAuth, 0);
		}

		public async override Task<FacebookUserInfo> GetUserInfo()
		{
			var me = _facebookClient.GetTaskAsync("me");
			return FacebookUserInfo.CreateFrom((IDictionary<string, object>) await me);
		}

		public void SaveAccessToken(string accessToken)
		{
			this._facebookClient.AccessToken = accessToken;
			this.SessionStatusSubject.OnNext(accessToken != null);
		}
    }
}

