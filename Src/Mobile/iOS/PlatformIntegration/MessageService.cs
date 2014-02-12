using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class MessageService : IMessageService
	{
		public const string ActionServiceMessage = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
		public const string ActionExtraMessage = "Mk_Taxi.ActionExtraMessage";

        readonly IMvxTouchViewPresenter _viewPresenter;

        public MessageService(IMvxTouchViewPresenter viewPresenter)
        {
            _viewPresenter = viewPresenter;
        }

		public Task ShowMessage(string title, string message)
		{
			return MessageHelper.Show( title, message );
		}

		public void ShowMessage(string title, string message, Action additionalAction )
		{
            MessageHelper.Show( title, message, "OK", additionalAction );
		}


        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {

            MessageHelper.Show( title, message,  positiveButtonTitle, positiveAction, negativeButtonTitle, negativeAction  );
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            MessageHelper.Show(title,message,positiveButtonTitle,positiveAction,negativeButtonTitle, negativeAction, neutralButtonTitle, neutralAction);
        }

        public void ShowMessage(string title, string message, List<KeyValuePair<string,Action>> additionalButton)
        {
            MessageHelper.Show( title, message,additionalButton);
        }

		public void ShowProgress( bool show )
		{
            if( show )
            {
                UIApplication.SharedApplication.InvokeOnMainThread ( () =>
                                                                    {               
                    LoadingOverlay.StartAnimatingLoading(   LoadingOverlayPosition.Center, null, 130, 30, null );
                });
            }
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread ( () =>
                                                                    {
                    LoadingOverlay.StopAnimatingLoading(  );
                });
            }
		}

        public IDisposable ShowProgress()
        {
            ShowProgress (true);
            return Disposable.Create (() => ShowProgress(false));
        }

        public void ShowDialogActivity(Type type)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(delegate {
                _viewPresenter.Show(new MvxViewModelRequest(type, null, null, MvxRequestedBy.UserAction));
            });
        }

        public void ShowDialog<T> (string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onItemSelected)
        {
            var enumerable = items as T[] ?? items.ToArray();
            var displayList = enumerable.Select(displayNameSelector).ToArray();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate {                               
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView ( title, string.Empty, null, null, displayList );
                av.Clicked += (sender, e) => {
                    onItemSelected(enumerable[e.ButtonIndex]);
                    av.Dispose();
                    };
                av.Show (  );
            });
                        
        }		

        public void ShowEditTextDialog(string title, string message, string positiveButtonTitle, Action<string> positionAction)
        {
        }

	}
}