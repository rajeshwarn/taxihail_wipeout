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
        void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction);
        void ShowMessage(string title, string message, List<KeyValuePair<string,Action>> additionalButton);
		void ShowProgress( bool show );
		void ShowProgress( bool show, Action cancel );

        void ShowToast(string message, ToastDuration duration);
		void ShowDialog<T>(string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onResult);
    }

    public enum ToastDuration
    {
        Short = 1,
        Long = 2,
    }
}
