using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Web;


namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social
{
	public class OAuthAuthorizerMonoTouch : OAuthAuthorizer
	{
		UIViewController _parent;

		public OAuthAuthorizerMonoTouch (OAuthConfig config) : base (config)
		{
			
		}

		/// <summary>
		/// Only need the first time when need the authorisation
		/// </summary>

		private Func<UIViewController> _getViewController;
		public OAuthAuthorizerMonoTouch (OAuthConfig config, Func<UIViewController> getViewController  ) : base (config)
		{
			_getViewController = getViewController;
			//_parent = getViewController();
		}


		class AuthorizationViewController : WebViewController {
			Action callback;
			OAuthAuthorizerMonoTouch container;
			string url;
			
			public AuthorizationViewController (OAuthAuthorizerMonoTouch oauth, string url, Action callback)
			{
				this.url = url;
				this.container = oauth;
				this.callback = callback;
			
				SetupWeb (url);
			}
				  
			protected override string UpdateTitle ()
			{
				return "Authorization";
			}
			
			public override void ViewWillAppear (bool animated)
			{
				SetupWeb ("Authorization");
				WebView.ShouldStartLoad = LoadHook;
				WebView.LoadRequest (new NSUrlRequest (new NSUrl (url)));
				base.ViewWillAppear (animated);
			}
			
			bool LoadHook (UIWebView sender, NSUrlRequest request, UIWebViewNavigationType navType)
			{
				var requestString = request.Url.AbsoluteString;
				if (requestString.StartsWith (container.config.Callback)){
					var results = HttpUtility.ParseQueryString (requestString.Substring (container.config.Callback.Length+1));
					
					container.AuthorizationToken = results ["oauth_token"];
					container.AuthorizationVerifier = results ["oauth_verifier"];
                    DismissViewController (false, () => {});

                    if (results["denied"] == null)
                    {
                        container.AcquireAccessToken();
                        callback ();
                    }
					
				}
				return true;
			}
		}
		
		public override void AuthorizeUser (Action callback)
		{
			_parent = _getViewController();

			var authweb = new AuthorizationViewController (this, config.AuthorizeUrl + "?oauth_token=" + RequestToken, callback);

			_parent.PresentModalViewController (authweb, true);
		}
	}
}