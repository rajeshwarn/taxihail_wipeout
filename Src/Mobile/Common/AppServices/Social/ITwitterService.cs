using System;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public interface ITwitterService
    {
		bool IsConnected { get; }
		void Connect();
		void GetUserInfos(Action<TwitterUserInfo> onRequestDone);
		void Share(string message, Action onRequestDone);
		void Disconnect();

		event EventHandler<object> ConnectionStatusChanged;
		void SetLoginContext(object context);
    }
}

