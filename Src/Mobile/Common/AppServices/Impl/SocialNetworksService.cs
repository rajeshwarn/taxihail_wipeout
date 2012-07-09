using System;
using SocialNetworks.Services.OAuth;

namespace TaxiMobileApp.Lib
{
	public class SocialNetworksService : ISocialNetworksService
	{
		public string FacebookAppId = "405057926196041";
		public string TwitterConsumerKey = "FDzRAIg828jnSPZkHx0hNQ";
		public string TwitterConsumerSecret = "KjIqnynywR1XHyy8iUlw8UVNWxZiCmCqrb2Dk4JtNoc";

		public SocialNetworksService ()
		{
		}

		public OAuthConfig GetOAuthConfig() 
		{
			return new OAuthConfig {
				ConsumerKey = TwitterConsumerKey,			
				Callback = "http://www.apcurium.com/oauth",
				ConsumerSecret = TwitterConsumerSecret,
				RequestTokenUrl = "https://api.twitter.com/oauth/request_token", 
				AccessTokenUrl = "https://api.twitter.com/oauth/access_token", 
				AuthorizeUrl = "https://api.twitter.com/oauth/authorize"
			};
		}
	}
}

