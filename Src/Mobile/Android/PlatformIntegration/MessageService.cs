using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Messages;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using TinyIoC;
using TinyMessenger;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {
        public MessageService(IMvxAndroidCurrentTopActivity context)
        {
            Context = context;
        }

        public IMvxAndroidCurrentTopActivity Context { get; set; }

        /// <summary>
        /// put the content of on activity on a modal dialog ( type = viewmodel Type )
        /// </summary>
        public void ShowDialog(Type type)
        {
            var presenter = new MvxAndroidViewPresenter();
			presenter.Show(new MvxViewModelRequest(type, null, null, MvxRequestedBy.UserAction));
        }

        public Task ShowMessage(string title, string message)
        {
            var dispatcher = TinyIoCContainer.Current.Resolve<IMvxViewDispatcher>();

            dispatcher.RequestMainThreadAction(() => AlertDialogHelper.Show(Context.Activity, title, message));

            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);

            return tcs.Task;
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction,
            string negativeButtonTitle, Action negativeAction)
        {
            AlertDialogHelper.Show(
                Context.Activity,
                title,
                message,
                positiveButtonTitle, (s,e) => positiveAction(),
                negativeButtonTitle, (s,e) => negativeAction());
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction,
            string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            AlertDialogHelper.Show(
                Context.Activity,
                title,
                message,
                positiveButtonTitle, (s,e) => positiveAction(),
                negativeButtonTitle, (s,e) => negativeAction(),
                neutralButtonTitle, (s,e) => neutralAction());
        }

        public void ShowMessage(string title, string message, List<KeyValuePair<string, Action>> additionalButton)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(string title, string message, Action additionalAction)
        {
            AlertDialogHelper.Show(Context.Activity, title, message, additionalAction);
        }

        public void ShowProgress(bool show)
        {
            if (show)
            {            
                LoadingOverlay.StartAnimatingLoading();
            }
            else
            {
                LoadingOverlay.StopAnimatingLoading();
            }

        }

        public void ShowProgressNonModal(bool show)
        {
            var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>();
            var rootView = topActivity.Activity.Window.DecorView.RootView as ViewGroup;

            if (rootView != null)
            {
                var progress = rootView.FindViewWithTag("Progress") as ProgressBar;

                if ((progress == null) && (show))
                {
                    var contentView = rootView.GetChildAt(0);
                    rootView.RemoveView(contentView);

                    var relLayout = new RelativeLayout(topActivity.Activity.ApplicationContext);
                    relLayout.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.FillParent);
                    relLayout.AddView(contentView);

                    var b = new ProgressBar(topActivity.Activity.ApplicationContext, null, Android.Resource.Attribute.ProgressBarStyleHorizontal)
                    {
                        Progress = 25,
                        LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.WrapContent),
                        Indeterminate = true,
                        Tag = "Progress"

                    };
                            
                    ((RelativeLayout.LayoutParams)b.LayoutParameters).TopMargin = 78.ToPixels();
                    relLayout.AddView(b);
                    rootView.AddView(relLayout);
                }               
                else if (progress != null)
                {
                    progress.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
                    progress.Indeterminate = true;
                    progress.Enabled = true;
                }
            }
        }

        public IDisposable ShowProgress()
        {
            ShowProgress(true);
            return Disposable.Create(() => ShowProgress(false));
        }

        public IDisposable ShowProgressNonModal()
        {
            ShowProgressNonModal (true);
            return Disposable.Create (() => ShowProgressNonModal(false));
        }

        public void ShowToast(string message, ToastDuration duration)
        {
            TinyIoCContainer.Current.Resolve<IMvxViewDispatcher>().RequestMainThreadAction(() =>
            {
                Toast toast = Toast.MakeText(Context.Activity, message,
                    duration == ToastDuration.Short ? ToastLength.Short : ToastLength.Long);
                toast.Show();
            });
        }

        public void ShowDialog<T>(string title, IEnumerable<T> items, Func<T, string> displayNameSelector,
            Action<T> onResult)
        {
            var messenger = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            var list = items.ToArray();
            if (displayNameSelector == null)
            {
                displayNameSelector = x => x.ToString();
            }
            if (onResult == null)
            {
                onResult = result => { };
            }

            string[] displayList = list.Select(displayNameSelector).ToArray();


            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context.Activity, typeof (SelectItemDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Items", displayList);
            i.PutExtra("OwnerId", ownerId);
            TinyMessageSubscriptionToken token = null;
// ReSharper disable once RedundantAssignment
            token = messenger.Subscribe<SubNavigationResultMessage<int>>(msg =>
            {
                // ReSharper disable AccessToModifiedClosure
                messenger.Unsubscribe<ActivityCompleted>(token);
                if (token != null) token.Dispose();
                // ReSharper restore AccessToModifiedClosure

                onResult(list[msg.Result]);
            },
                msg => msg.MessageId == ownerId);
            Context.Activity.StartActivity(i);
        }

		public Task<string> ShowPromptDialog(string title, string message, Action cancelAction)
        {
			var tcs = new TaskCompletionSource<string> ();

			var ownerId = Guid.NewGuid().ToString();
			var i = new Intent(Context.Activity, typeof(EditTextDialogActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
			i.PutExtra("Title", title);
			i.PutExtra("Message", message);
			i.PutExtra("PositiveButtonTitle", Context.Activity.GetString(Resource.String.OkButtonText));
			i.PutExtra("NegativeButtonTitle", Context.Activity.GetString(Resource.String.Cancel));
			i.PutExtra("OwnerId", ownerId);

			var messenger = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();

			TinyMessageSubscriptionToken token = null;
			token = messenger.Subscribe<ActivityCompleted>(a =>
			{
				if(a.Content == null)
				{
					tcs.TrySetCanceled ();
					cancelAction ();
				}
				else
				{
					tcs.TrySetResult (a.Content);
				}

				messenger.Unsubscribe<ActivityCompleted>(token);
				token.Dispose();
			}, a => a.OwnerId == ownerId);

			Context.Activity.StartActivity(i);

			return tcs.Task;
        }
    }
}