using System;
namespace SocialNetworks.Services.OAuth
{
	public class OAuthConfig
	{
		// keys, callbacks
		public string ConsumerKey, Callback, ConsumerSecret, TwitPicKey, BitlyKey;
		
		// Urls
		public string RequestTokenUrl, AccessTokenUrl, AuthorizeUrl;
	}
}

