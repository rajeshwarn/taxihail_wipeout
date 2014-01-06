using System;
using Android.Content;
using Android.App;
using Facebook;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Impl;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class FacebookService: FacebookServiceBase
    {
		readonly string _appId;
		readonly Activity _mainActivity;

		public FacebookService(string appId, Activity mainActivity)
        {
			this._mainActivity = mainActivity;
			this._appId = appId;
        }

		public override void Connect(string permissions)
		{
			var webAuth = new Intent (_mainActivity, typeof (FacebookWebViewAuthActivity));
			webAuth.PutExtra ("AppId", _appId);
			webAuth.PutExtra ("ExtendedPermissions", permissions);
			_mainActivity.StartActivityForResult (webAuth, 0);
		}
    }
}

