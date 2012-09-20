using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class MessageService : IMessageService
	{
		public const string ACTION_SERVICE_MESSAGE = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
		public const string ACTION_EXTRA_MESSAGE = "Mk_Taxi.ACTION_EXTRA_MESSAGE";
		
		public MessageService()
		{

		}

		public void ShowMessage(string title, string message)
		{
			MessageHelper.Show( title, message );
		}

		public void ShowMessage(string title, string message, string additionnalActionButtonTitle, Action additionalAction )
		{
			MessageHelper.Show( title, message, additionnalActionButtonTitle, additionalAction );
		}

		public void ShowToast(string message, ToastDuration duration)
		{}
	}
}