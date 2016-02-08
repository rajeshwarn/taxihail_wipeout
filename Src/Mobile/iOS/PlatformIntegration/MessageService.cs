using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Infrastructure;
using UIKit;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
	{
		public const string ActionServiceMessage = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
		public const string ActionExtraMessage = "Mk_Taxi.ActionExtraMessage";

        readonly IMvxTouchViewPresenter _viewPresenter;

		readonly ICacheService _cacheService;

        public MessageService(IMvxTouchViewPresenter viewPresenter, ICacheService cacheService)
        {
            _viewPresenter = viewPresenter;
			_cacheService = cacheService;
        }

		public Task ShowMessage(string title, string message)
		{
			return MessageHelper.Show(title, message);
		}

		public Task ShowMessage(string title, string message, Action additionalAction)
		{
            return MessageHelper.Show(title, message, Localize.GetValue("OkButtonText"), additionalAction);
		}

        public Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
            return MessageHelper.Show(title, message, positiveButtonTitle, positiveAction, negativeButtonTitle, negativeAction);
        }

        public Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            return MessageHelper.Show(title, message, positiveButtonTitle, positiveAction, negativeButtonTitle, negativeAction, neutralButtonTitle, neutralAction);
        }
		public void ShowProgress(bool show)
		{
            if(show)
            {
				var cachedServiceType = _cacheService.Get<string> ("ServiceTypeForProgressAnimation");
				var isLuxury = cachedServiceType != null && cachedServiceType == "Luxury";
                UIApplication.SharedApplication.InvokeOnMainThread (() => {               
					LoadingOverlay.StartAnimatingLoading(isLuxury);
                });
            }
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread (() => {
                    LoadingOverlay.StopAnimatingLoading();
                });
            }
		}

        public void ShowProgressNonModal(bool show)
        {
            if(show)
            {
                UIApplication.SharedApplication.InvokeOnMainThread (() => {     
                    LoadingBar.Show();
                });
            }
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread (() => {
                    LoadingBar.Hide();
                });
            }
        }

        public IDisposable ShowProgress()
        {
            ShowProgress (true);
            return Disposable.Create (() => ShowProgress(false));
        }

        public IDisposable ShowProgressNonModal()
        {
            ShowProgressNonModal (true);
            return Disposable.Create (() => ShowProgressNonModal(false));
        }

        public void ShowDialog(Type type)
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
                av.Show ();
            });     
        }		

        public Task<string> ShowPromptDialog(string title, string message, Action cancelAction, bool isNumericOnly = false, string inputText = "")
        {
            return MessageHelper.Prompt (title, message, cancelAction, isNumericOnly, inputText);
        }
	}
}