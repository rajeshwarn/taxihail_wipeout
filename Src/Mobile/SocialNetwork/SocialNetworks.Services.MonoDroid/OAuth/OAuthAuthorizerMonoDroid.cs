using System;
using Android.App;
using Android.Views;
using Android.Webkit;
using Android.Content;
using System.Web;
using Android.Widget;

namespace SocialNetworks.Services.OAuth
{
	public class OAuthAuthorizerMonoDroid : OAuthAuthorizer
	{
		Activity _parent;
		//int _layoutRes;

		public OAuthAuthorizerMonoDroid (OAuthConfig config, Activity parent) : base(config)
		{
			_parent = parent;
		}

		#region implemented abstract members of SocialNetworks.Services.OAuth.OAuthAuthorizer
		public override void AuthorizeUser (Action callback)
		{
			//AlertDialog.Builder dialog = new AlertDialog.Builder(_parent);
            Dialog dialog = new Dialog(this._parent);
			dialog.SetTitle("twitter authentication");
			WebView webView = new WebView(_parent);
            LinearLayout ln = new LinearLayout(_parent);
            ln.AddView(webView);
            dialog.SetContentView(ln);

			//dialog.SetNegativeButton("close", new CloseListener());
            dialog.SetCancelable(true);

			//var alertDialog = dialog.Create();

            var webViewClient = new OAuthWebViewClient(this, callback, dialog);
			webView.SetWebViewClient(webViewClient);

			webView.LoadUrl(config.AuthorizeUrl + "?oauth_token=" + RequestToken);

            webView.RequestFocus(FocusSearchDirection.Down);
		    webView.Touch += (v, e) =>
		                         {
		                             var view = v as View;
		                             switch (e.Event.Action)
		                             {
		                                 case MotionEventActions.Down:
		                                 case MotionEventActions.Up:
		                                     if (!view.HasFocus)
		                                     {
		                                         view.RequestFocus();
		                                     }
		                                     break;
		                             }
		                             e.Handled = false;
		                         };

            dialog.Show();
            
		}
		#endregion

		class OAuthWebViewClient : WebViewClient
		{
			Action _callback;
			OAuthAuthorizerMonoDroid _container;
			Dialog _dialog;

			public OAuthWebViewClient (OAuthAuthorizerMonoDroid container, Action callback, Dialog dialog) : base()
			{
				_callback = callback;
				_container = container;
				_dialog = dialog;
			}

			public override bool ShouldOverrideUrlLoading(WebView view, String url)
            {
                view.LoadUrl(url);
                return true;
            }

			public override void OnPageStarted (WebView view, string url, Android.Graphics.Bitmap favicon)
			{
				base.OnPageStarted (view, url, favicon);
				if (url.StartsWith (_container.config.Callback)){

					var results = HttpUtility.ParseQueryString (url.Substring (_container.config.Callback.Length+1));
                    if(results["denied"] == null)
                    {
                        _container.AuthorizationToken = results["oauth_token"];
                        _container.AuthorizationVerifier = results["oauth_verifier"];
                        _container.AcquireAccessToken();
                        _callback();
                    }
					_dialog.Dismiss();
				}
			}
		}

		class CloseListener : Java.Lang.Object, IDialogInterfaceOnClickListener
		{
			public void OnClick (IDialogInterface dialog, int which){
				dialog.Dismiss();
			}
		}
	}
}

