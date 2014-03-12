using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Booking.Mobile.Client.Services.Social
{
	public class TwitterServiceMonoDroid : TwitterServiceBase
	{
		Activity _parent;
		const string KEY = "twitter-session";

        public TwitterServiceMonoDroid (OAuthConfig oauthConfig, IMvxAndroidCurrentTopActivity topActivity) : base(oauthConfig)
		{
            _parent = topActivity.Activity;
			LoadCredentials();
		}

		public override void SetLoginContext(object context)
		{
			base.SetLoginContext(context);
			_parent = context as Activity;
		}

		protected override void ClearAuthorization ()
		{
			base.ClearAuthorization ();
			var editor = _parent.GetSharedPreferences (KEY, FileCreationMode.Private).Edit ();
			editor.Clear ();
			editor.Commit ();
		}

		#region implemented abstract members of SocialNetworks.Services.TwitterServiceBase
		protected override void LoadCredentials ()
		{
			var savedSession = _parent.GetSharedPreferences (KEY, FileCreationMode.Private);
			if(savedSession != null)
			{
				_oAuthToken = savedSession.GetString (TWOAuthToken, null);
				_oAuthTokenSecret = savedSession.GetString (TWOAuthTokenSecret, null);
				_userId = savedSession.GetString (TWOAccessId, null);
				_screenName = savedSession.GetString (TWOAccessScreenName, null);
			}
		}

		protected override void SaveCredentials (OAuthAuthorizer oauth)
		{
			var editor = _parent.GetSharedPreferences (KEY,FileCreationMode.Private).Edit ();
			editor.PutString (TWOAuthToken, oauth.AccessToken);
			editor.PutString (TWOAuthTokenSecret, oauth.AccessTokenSecret);
			editor.PutString (TWOAccessId, oauth.AccessId.ToString ());
			editor.PutString (TWOAccessScreenName, oauth.AccessScreenname);
			editor.Commit ();
		}

		protected override OAuthAuthorizer GetOAuthAuthorizer ()
		{
			return new OAuthAuthorizerMonoDroid(_oauthConfig, _parent);
		}
		#endregion

	}
}

