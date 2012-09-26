using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;

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


        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
            MessageHelper.Show( title, message, positiveButtonTitle, positiveAction );
        }


		private UIViewController _owner;
		public void ShowProgress( bool show, Action cancel )
		{
			if( show )
			{
				_owner = AppContext.Current.Controller.TopViewController;
				LoadingOverlay.StartAnimatingLoading(  _owner.View, LoadingOverlayPosition.Center, null, 130, 30, cancel );
			}
			else
			{
				LoadingOverlay.StopAnimatingLoading( _owner.View );
			}
		}

		public void ShowProgress( bool show )
		{
			ShowProgress( show, null );
		}

		public void ShowToast(string message, ToastDuration duration)
		{}

	}
}