using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Collections.Specialized;


namespace apcurium.MK.Booking.Mobile.AppServices.Social.OAuth
{
	
	// The authorizer uses a config and an optional xAuth user/password
	// to perform the OAuth authorization process as well as signing
	// outgoing http requests
	//
	// To get an access token, you use these methods in the workflow:
	// 	  AcquireRequestToken
	//    AuthorizeUser
	//
	// These static methods only require the access token:
	//    AuthorizeRequest
	//    AuthorizeTwitPic
	//
	public abstract class OAuthAuthorizer {
		// Settable by the user
		public string xAuthUsername, xAuthPassword;
		
		protected OAuthConfig config;
		protected string RequestToken, RequestTokenSecret;
		protected string AuthorizationToken, AuthorizationVerifier;
		public string AccessToken, AccessTokenSecret, AccessScreenname;
		public long AccessId;
		
		// Constructor for standard OAuth
		public OAuthAuthorizer (OAuthConfig config)
		{
			this.config = config;
		}
		
		// Constructor for xAuth
		public OAuthAuthorizer (OAuthConfig config, string xAuthUsername, string xAuthPassword)
		{
			this.config = config;
			this.xAuthUsername = xAuthUsername;
			this.xAuthPassword = xAuthPassword;
		}

		static Random random = new Random ();
		static DateTime UnixBaseTime = new DateTime (1970, 1, 1);

		// 16-byte lower-case or digit string
		static string MakeNonce ()
		{
			var ret = new char [16];
			for (int i = 0; i < ret.Length; i++){
				int n = random.Next (35);
				if (n < 10)
					ret [i] = (char) (n + '0');
				else
					ret [i] = (char) (n-10 + 'a');
			}
			return new string (ret);
		}
		
		static string MakeTimestamp ()
		{
			return ((long) (DateTime.UtcNow - UnixBaseTime).TotalSeconds).ToString ();
		}
		
		// Makes an OAuth signature out of the HTTP method, the base URI and the headers
		static string MakeSignature (string method, string base_uri, Dictionary<string,string> headers)
		{
			var items = from k in headers.Keys orderby k 
			            select k + "%3D" + OAuthEncoder.PercentEncode (headers [k]);

			return method + "&" + OAuthEncoder.PercentEncode (base_uri) + "&" + 
				string.Join ("%26", items.ToArray ());
		}
		
		static string MakeSigningKey (string consumerSecret, string oauthTokenSecret)
		{
			return OAuthEncoder.PercentEncode (consumerSecret) + "&" + (oauthTokenSecret != null ? OAuthEncoder.PercentEncode (oauthTokenSecret) : "");
		}
		
		static string MakeOAuthSignature (string compositeSigningKey, string signatureBase)
		{
			var sha1 = new HMACSHA1 (Encoding.UTF8.GetBytes (compositeSigningKey));
			
			return Convert.ToBase64String (sha1.ComputeHash (Encoding.UTF8.GetBytes (signatureBase)));
		}
		
		static string HeadersToOAuth (Dictionary<string,string> headers)
		{
			return "OAuth " + String.Join (",", (from x in headers.Keys select String.Format ("{0}=\"{1}\"", x, headers [x])).ToArray ());
		}
		
		public bool AcquireRequestToken ()
		{
			var headers = new Dictionary<string,string> () {
				{ "oauth_callback", OAuthEncoder.PercentEncode (config.Callback) },
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};
				
			string signature = MakeSignature ("POST", config.RequestTokenUrl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, null);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);
			
			var wc = new WebClient ();
			headers.Add ("oauth_signature", OAuthEncoder.PercentEncode (oauth_signature));
			wc.Headers [HttpRequestHeader.Authorization] = HeadersToOAuth (headers);
			
			try {
				var result = HttpUtility.ParseQueryString (wc.UploadString (new Uri (config.RequestTokenUrl), ""));

				if (result ["oauth_callback_confirmed"] != null){
					RequestToken = result ["oauth_token"];
					RequestTokenSecret = result ["oauth_token_secret"];
					
					return true;
				}
			} catch (Exception e) {
				Console.WriteLine (e);
				// fallthrough for errors
			}
			return false;
		}
		
		// Invoked after the user has authorized us
		//
		// TODO: this should return the stream error for invalid passwords instead of
		// just true/false.
		public bool AcquireAccessToken ()
		{
			var headers = new Dictionary<string,string> () {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};
			var content = "";
			if (xAuthUsername == null){
				headers.Add ("oauth_token", OAuthEncoder.PercentEncode (AuthorizationToken));
				headers.Add ("oauth_verifier", OAuthEncoder.PercentEncode (AuthorizationVerifier));
			} else {
				headers.Add ("x_auth_username", OAuthEncoder.PercentEncode (xAuthUsername));
				headers.Add ("x_auth_password", OAuthEncoder.PercentEncode (xAuthPassword));
				headers.Add ("x_auth_mode", "client_auth");
				content = String.Format ("x_auth_mode=client_auth&x_auth_password={0}&x_auth_username={1}", OAuthEncoder.PercentEncode (xAuthPassword), OAuthEncoder.PercentEncode (xAuthUsername));
			}
			
			string signature = MakeSignature ("POST", config.AccessTokenUrl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, RequestTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);
			
			var wc = new WebClient ();
			headers.Add ("oauth_signature", OAuthEncoder.PercentEncode (oauth_signature));
			if (xAuthUsername != null){
				headers.Remove ("x_auth_username");
				headers.Remove ("x_auth_password");
				headers.Remove ("x_auth_mode");
			}
			wc.Headers [HttpRequestHeader.Authorization] = HeadersToOAuth (headers);
			
			try {
				var result = HttpUtility.ParseQueryString (wc.UploadString (new Uri (config.AccessTokenUrl), content));

				if (result ["oauth_token"] != null){
					AccessToken = result ["oauth_token"];
					AccessTokenSecret = result ["oauth_token_secret"];
					AccessScreenname = result ["screen_name"];
					AccessId = Int64.Parse (result ["user_id"]);
					
					return true;
				}
			} catch (WebException e) {
				var x = e.Response.GetResponseStream ();
				var j = new System.IO.StreamReader (x);
				Console.WriteLine (j.ReadToEnd ());
				Console.WriteLine (e);
				// fallthrough for errors
			}
			return false;
		}
		
		// 
		// Assign the result to the Authorization header, like this:
		// request.Headers [HttpRequestHeader.Authorization] = AuthorizeRequest (...)
		//
		public static string AuthorizeRequest (OAuthConfig config, string oauthToken, string oauthTokenSecret, string method, Uri uri, string data)
		{
			var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" }};
			var signatureHeaders = new Dictionary<string,string> (headers);

			// Add the data and URL query string to the copy of the headers for computing the signature
			if (data != null && data != ""){
				var parsed = HttpUtility.ParseQueryString (data);
				foreach (string k in parsed.Keys){
					signatureHeaders.Add (k, OAuthEncoder.PercentEncode (parsed [k]));
				}
			}
			
			var nvc = HttpUtility.ParseQueryString (uri.Query);
			foreach (string key in nvc){
				if (key != null)
					signatureHeaders.Add (key, OAuthEncoder.PercentEncode (nvc [key]));
			}
			
			string signature = MakeSignature (method, uri.GetLeftPart (UriPartial.Path), signatureHeaders);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, oauthTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);

			headers.Add ("oauth_signature", OAuthEncoder.PercentEncode (oauth_signature));
			
			return HeadersToOAuth (headers);
		}

		//
		// Used to authorize an HTTP request going to TwitPic
		//
		public static void AuthorizeTwitPic (OAuthConfig config, HttpWebRequest wc, string oauthToken, string oauthTokenSecret)
		{
			var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" },
				//{ "realm", "http://api.twitter.com" }
			};
			string signurl = "http://api.twitter.com/1.1/account/verify_credentials.xml";
			// The signature is not done against the *actual* url, it is done against the verify_credentials.json one 
			string signature = MakeSignature ("GET", signurl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, oauthTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);

			headers.Add ("oauth_signature", OAuthEncoder.PercentEncode (oauth_signature));

			
			wc.Headers.Add ("X-Verify-Credentials-Authorization", HeadersToOAuth (headers));
			wc.Headers.Add ("X-Auth-Service-Provider", signurl);
		}	

		public abstract void AuthorizeUser (Action callback);

	}
	
}

