using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IMessageService
    {
		Task ShowMessage(string title, string message);
		Task ShowMessage(string title, string message, Action additionalAction );
		Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction);
        Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction);

		void ShowProgress(bool show);
		void ShowProgressNonModal(bool show);
		IDisposable ShowProgress ();
		IDisposable ShowProgressNonModal ();
        
        void ShowDialog(Type type);
		void ShowDialog<T>(string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onResult);
		Task<string> ShowPromptDialog(string title, string message, Action cancelAction, bool isNumericOnly = false, string inputText = "");
    }

    public enum ToastDuration
    {
        Short = 0,
        Long = 1,
    }
}
