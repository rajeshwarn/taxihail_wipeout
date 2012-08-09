using System;
using SocialNetworks.Services.Entities;
using System.Collections.Generic;


namespace SocialNetworks.Services
{
	public interface IFacebookService
	{
		bool IsConnected { get; }
		void Connect(string permissions);
		void GetUserInfos(Action<UserInfos> onRequestDone, Action onError);
		void GetLikes( Action<List<UserLike>> onRequestDone );
	    void SetCurrentContext(object context);

		void Share(Post post, Action onRequestDone);
		void Like( string objectId );
		void Disconnect();
		
		event EventHandler<FacebookStatus> ConnectionStatusChanged;
	}
}

