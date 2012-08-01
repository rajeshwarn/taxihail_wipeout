using System;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.OAuth;


namespace SocialNetworks.Services
{
	public interface ITwitterService
	{
		bool IsConnected { get; }
		void Connect();
		void GetUserInfos(Action<UserInfos> onRequestDone);
		void Share(string message, Action onRequestDone);
		void Disconnect();
		
		event EventHandler<TwitterStatus> ConnectionStatusChanged;
	    void SetLoginContext(object context);
	}
}

