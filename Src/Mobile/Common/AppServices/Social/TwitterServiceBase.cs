using System;
using System.Json;
using System.Net;
using System.Text;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public abstract class TwitterServiceBase : ITwitterService
	{
		protected OAuthConfig _oauthConfig;
		protected const string TWOAuthToken = "TWAOAuthToken";
		protected const string TWOAuthTokenSecret = "TWOAuthTokenSecret";
		protected const string TWOAccessId = "TWOAccessId";
		protected const string TWOAccessScreenName = "TWOAccessScreenName";
		protected string _oAuthToken;
		protected string _oAuthTokenSecret;
		protected string _userId;
		protected string _screenName;


		public TwitterServiceBase (OAuthConfig oauthConfig)
		{
			_oauthConfig = oauthConfig;
		}

		protected virtual void ClearAuthorization ()
		{
			_oAuthToken = null;
			_oAuthTokenSecret = null;
			_userId = null;
			_screenName = null;
		}

		protected abstract void LoadCredentials();

		protected abstract void SaveCredentials (OAuthAuthorizer oauth);

		protected abstract OAuthAuthorizer GetOAuthAuthorizer();

		WebClient GetClient ()
		{
			return new WebClient (); 
		}

		#region ITwitterService implementation
		public event EventHandler<TwitterStatus> ConnectionStatusChanged;

		public virtual void SetLoginContext(object context)
		{

		}

		public void Connect ()
		{
			if (_oAuthToken == null && _oAuthTokenSecret == null)
			{
				var oauth = GetOAuthAuthorizer();
				if (oauth.AcquireRequestToken ()){
					oauth.AuthorizeUser (delegate {

						SaveCredentials(oauth);
						_oAuthToken = oauth.AccessToken;
						_oAuthTokenSecret = oauth.AccessTokenSecret;
						_userId = oauth.AccessId.ToString();
						_screenName = oauth.AccessScreenname;
						if( ConnectionStatusChanged != null )
						{
							ConnectionStatusChanged(this, new TwitterStatus(true));
						}
					});
				}
			}
		}

		public void GetUserInfos (Action<TwitterUserInfo> onRequestDone)
		{
			var content = string.Format("user_id={0}&screen_name={1}", OAuthEncoder.PercentEncode (_userId), OAuthEncoder.PercentEncode (_screenName)) ;
			var taskUri = new Uri("https://api.twitter.com/1.1/users/show.json?" + content);
			var request = (HttpWebRequest) WebRequest.Create (taskUri);
			request.AutomaticDecompression = DecompressionMethods.GZip;		
			request.Headers [HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest (_oauthConfig, _oAuthToken, _oAuthTokenSecret, "GET", taskUri, null);

			TwitterUserInfo userInfos = null;

			request.BeginGetResponse (ar => {
				try {
					var response = (HttpWebResponse) request.EndGetResponse (ar);
					using(var stream = response.GetResponseStream ())
					{
						#if !WINDOWS
						var jentry = (JsonObject) JsonValue.Load (stream);
						userInfos = new TwitterUserInfo();
						userInfos.Id = jentry["id_str"];
						string name = jentry["name"];
						if( name.Contains(" ") )
						{
							userInfos.Firstname = name.Substring( 0, name.IndexOf( " " ) );
							userInfos.Lastname = name.Substring( name.IndexOf( " " ) + 1 );
						}
						else
						{
							userInfos.Firstname = name;
						}
						userInfos.City = jentry["location"];
						#endif

					}						
				} catch (WebException we){
					var response = we.Response as HttpWebResponse;
					if (response != null){
						switch (response.StatusCode){
							case HttpStatusCode.Unauthorized:
								// This is the case of sharing two keys
								break;
						}

					}
					Console.WriteLine (we);					
				}	
				finally{
					if(onRequestDone != null)
					{
						onRequestDone(userInfos);
					}
				}

			}, null);
		}

		public void Share (string message, Action onRequestDone)
		{
			var content = string.Format("status={0}&trim_user=true&include_entities=true", OAuthEncoder.PercentEncode (message)) ;
			var taskUri = new Uri("https://api.twitter.com/1/statuses/update.json");
			var client = GetClient ();

			client.Headers [HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest (_oauthConfig, _oAuthToken, _oAuthTokenSecret, "POST", taskUri, content);

			try
			{				
				client.UploadData (taskUri, "POST", Encoding.UTF8.GetBytes (content));	
				if(onRequestDone != null)
				{
					onRequestDone();
				}
			}
			catch( WebException ex )
			{
				Console.WriteLine( "Error while posting on Twitter. Message: " + ex.Message );
			}
			finally
			{
				client.Dispose();
			}

		}

		public void Disconnect ()
		{
			ClearAuthorization();
			ConnectionStatusChanged(this, new TwitterStatus(false));
		}

		public bool IsConnected {
			get { return _oAuthToken != null && _oAuthTokenSecret != null; }
		}
		#endregion
	}
}

