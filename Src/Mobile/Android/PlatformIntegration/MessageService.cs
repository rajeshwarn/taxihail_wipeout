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

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {
        private readonly Stack<ProgressDialog> _progressDialogs = new Stack<ProgressDialog>();
        
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
            TinyIoCContainer.Current.Resolve<IMvxViewDispatcher>().RequestMainThreadAction(() =>
            {
                var activity = TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

                if (show)
                {
                    var progress = new ProgressDialog(activity);
                    _progressDialogs.Push(progress);
                    progress.SetTitle(string.Empty);
                    progress.SetMessage(activity.GetString(Resource.String.LoadingMessage));
                    progress.Show();
                }
                else
                {
                    if (_progressDialogs.Any())
                    {
                        var progressPrevious = _progressDialogs.Pop();
                        if (progressPrevious != null
                            && progressPrevious.IsShowing)
                        {
                            try
                            {
                                progressPrevious.Dismiss();
                            }
// ReSharper disable once EmptyGeneralCatchClause
                            catch
                            {
                            } // on peut avoir une exception ici si activity est plus présente, pas grave
                        }
                    }
                }
            });
        }

        public IDisposable ShowProgress()
        {
            ShowProgress(true);
            return Disposable.Create(() => ShowProgress(false));
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