using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client.Messages;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Views;
using TinyIoC;
using TinyMessenger;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Message;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {
        public MessageService(Context context)
        {
            Context = context;
        }

        public Context Context { get; set; }

        /// <summary>
        /// put the content of on activity on a modal dialog ( type = viewmodel Type )
        /// </summary>
        public void ShowDialogActivity(Type type)
        {
            var presenter = new MvxAndroidViewPresenter();
			presenter.Show(new MvxViewModelRequest(type, null, null, MvxRequestedBy.UserAction));
        }

        public Task ShowMessage(string title, string message)
        {
            var ownerId = Guid.NewGuid().ToString();
            var dispatcher = TinyIoCContainer.Current.Resolve<IMvxViewDispatcher>();
            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();

            dispatcher.RequestMainThreadAction(() =>
            {
                var i = new Intent(Context, typeof (AlertDialogActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
                i.PutExtra("Title", title);
                i.PutExtra("Message", message);
                i.PutExtra("OwnerId", ownerId);
                Context.StartActivity(i);
            });

            var tcs = new TaskCompletionSource<object>();
            TinyMessageSubscriptionToken token = null;
// ReSharper disable once RedundantAssignment
            token = messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                tcs.TrySetResult(null);
// ReSharper disable AccessToModifiedClosure
                messengerHub.Unsubscribe<ActivityCompleted>(token);
                if (token != null) token.Dispose();
// ReSharper restore AccessToModifiedClosure
            }, a => a.OwnerId == ownerId);

            return tcs.Task;
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction,
            string negativeButtonTitle, Action negativeAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof (AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            TinyMessageSubscriptionToken token = null;
// ReSharper disable once RedundantAssignment
            token = messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                if (a.Content == positiveButtonTitle && positiveAction != null)
                {
                    positiveAction();
                }
                else if (a.Content == negativeButtonTitle && negativeAction != null)
                {
                    negativeAction();
                }
                // ReSharper disable AccessToModifiedClosure
                messengerHub.Unsubscribe<ActivityCompleted>(token);
                if (token != null) token.Dispose();
                // ReSharper restore AccessToModifiedClosure
            }, a => a.OwnerId == ownerId);

            Context.StartActivity(i);
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction,
            string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof (AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("NeutralButtonTitle", neutralButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            TinyMessageSubscriptionToken token = null;
// ReSharper disable once RedundantAssignment
            token = messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                if (a.Content == positiveButtonTitle)
                {
                    positiveAction();
                }
                else if (a.Content == negativeButtonTitle)
                {
                    negativeAction();
                }
                else if (a.Content == neutralButtonTitle)
                {
                    neutralAction();
                }
                // ReSharper disable AccessToModifiedClosure
                messengerHub.Unsubscribe<ActivityCompleted>(token);
                if (token != null) token.Dispose();
                // ReSharper restore AccessToModifiedClosure
            }, a => a.OwnerId == ownerId);

            Context.StartActivity(i);
        }

        public void ShowMessage(string title, string message, List<KeyValuePair<string, Action>> additionalButton)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(string title, string message, Action additionalAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof (AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("NeutralButtonTitle", "OK");
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
// ReSharper disable once RedundantAssignment
            token = messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                additionalAction();
                // ReSharper disable AccessToModifiedClosure
                messengerHub.Unsubscribe<ActivityCompleted>(token);
                if (token != null) token.Dispose();
                // ReSharper restore AccessToModifiedClosure
            }, a => a.OwnerId == ownerId);

            Context.StartActivity(i);
        }

        public void ShowProgress(bool show)
        {
            var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>();
            var rootView = topActivity.Activity.Window.DecorView.RootView as ViewGroup;

            if (rootView != null)
            {
                if (show)
                {
                    var contentView = rootView.GetChildAt(0);
                    rootView.RemoveView(contentView);
                    var relLayout = new RelativeLayout(topActivity.Activity.ApplicationContext);
                    relLayout.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.FillParent);
                    relLayout.AddView(contentView);
                    LoadingOverlay.StartAnimatingLoading(relLayout, topActivity.Activity);
                    rootView.AddView(relLayout);
                }
                else
                {
                    LoadingOverlay.StopAnimatingLoading();
                }
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
                            
                    ((RelativeLayout.LayoutParams)b.LayoutParameters).TopMargin = 92.ToPixels();
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
                Toast toast = Toast.MakeText(Context, message,
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
            var i = new Intent(Context, typeof (SelectItemDialogActivity));
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
            Context.StartActivity(i);
        }

        public void ShowEditTextDialog(string title, string message, string positiveButtonTitle,
            Action<string> positionAction)
        {
            throw new NotImplementedException();
        }
    }
}