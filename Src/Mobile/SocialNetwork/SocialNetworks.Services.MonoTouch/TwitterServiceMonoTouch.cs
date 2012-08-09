using System;
using SocialNetworks.Services.OAuth;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SocialNetworks.Services.MonoTouch
{
	public class TwitterServiceMonoTouch : TwitterServiceBase
	{
		private Func<UIViewController> _getViewController;

		public TwitterServiceMonoTouch (OAuthConfig oauthConfig, Func<UIViewController> getViewController ) : base(oauthConfig)
		{
			_getViewController = getViewController;
			LoadCredentials();
		}

		protected override OAuthAuthorizer GetOAuthAuthorizer( )
		{
			return new OAuthAuthorizerMonoTouch( _oauthConfig, _getViewController );
		}

		protected override void LoadCredentials()
		{
			var defaults = NSUserDefaults.StandardUserDefaults;
			if(defaults [TWOAuthToken] != null && defaults [TWOAuthTokenSecret] != null) {
				_oAuthToken = defaults [TWOAuthToken] as NSString;
				_oAuthTokenSecret = defaults [TWOAuthTokenSecret] as NSString;
				_userId = defaults [TWOAccessId] as NSString;
				_screenName = defaults [TWOAccessScreenName] as NSString;
			}
		}

		protected  override void SaveCredentials (OAuthAuthorizer oauth)
		{
			var defaults = NSUserDefaults.StandardUserDefaults;
			defaults [TWOAuthToken] = new NSString (oauth.AccessToken);
			defaults [TWOAuthTokenSecret] = new NSString (oauth.AccessTokenSecret);
			defaults [TWOAccessId] = new NSString (oauth.AccessId.ToString ());
			defaults [TWOAccessScreenName] = new NSString (oauth.AccessScreenname);
			defaults.Synchronize ();
		}

        protected override void ClearAuthorization ()
        {
			base.ClearAuthorization();
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.RemoveObject(TWOAuthToken);
            defaults.RemoveObject(TWOAuthTokenSecret);
            defaults.RemoveObject(TWOAccessId);
            defaults.RemoveObject(TWOAccessScreenName);
            defaults.Synchronize();
        } 

	}
}

