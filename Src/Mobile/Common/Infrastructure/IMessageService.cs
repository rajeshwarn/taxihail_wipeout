using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IMessageService
    {
        void ShowMessage(string title, string message);
		void ShowMessage(string title, string message, string additionnalActionButtonTitle, Action additionalAction );
        void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction);
		void ShowProgress( bool show );

        void ShowToast(string message, ToastDuration duration);
    }

    public enum ToastDuration
    {
        Short,
        Long,
    }
}
