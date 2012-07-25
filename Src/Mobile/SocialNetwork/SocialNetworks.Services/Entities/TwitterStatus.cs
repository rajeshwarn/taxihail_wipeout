using System;
namespace SocialNetworks.Services.Entities
{
	public class TwitterStatus : EventArgs
	{
		public bool IsConnected { get; set; }

		public TwitterStatus (bool isConnected)
		{
			IsConnected = isConnected;
		}
	}

}

