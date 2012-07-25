using System;

namespace SocialNetworks.Services.Entities
{
	public class FacebookStatus : EventArgs
	{
		public bool IsConnected { get; set; }

		public FacebookStatus (bool isConnected)
		{
			IsConnected = isConnected;
		}
	}

}

