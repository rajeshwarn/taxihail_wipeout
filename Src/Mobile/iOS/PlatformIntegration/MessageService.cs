using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class MessageService : IMessageService
	{
		public const string ACTION_SERVICE_MESSAGE = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
		public const string ACTION_EXTRA_MESSAGE = "Mk_Taxi.ACTION_EXTRA_MESSAGE";
		
		public MessageService()
		{

		}

        #region IMessageService implementation

      
        #endregion

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
                var presenter = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxTouchViewPresenter>();
                presenter.Show(new MvxShowViewModelRequest(type,null, false, MvxRequestedBy.UserAction));
            });
        }

        public void ShowDialog<T> (string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onItemSelected)
        {
            var list = items.ToList();
            var displayList = items.Select(displayNameSelector).ToArray();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate {                               
                LoadingOverlay.StopAnimatingLoading();
                var av = new UIAlertView ( title, string.Empty, null, null, displayList );
                av.Clicked += (object sender, UIButtonEventArgs e) => {
                    onItemSelected(list[e.ButtonIndex]);
                    av.Dispose();
                    };
                av.Show (  );
            });
                        
        }

		public void ShowToast(string message, ToastDuration duration)
		{
			MessageHelper.ShowToast(message, (int)duration );
		}

        public void ShowEditTextDialog(string title, string message, string positiveButtonTitle, Action<string> positionAction)
        {
        }

	}
}