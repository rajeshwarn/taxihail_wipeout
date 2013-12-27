using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using TinyMessenger;
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Android.Interfaces;
using apcurium.MK.Callbox.Mobile.Client.Activities;
using apcurium.MK.Callbox.Mobile.Client.Messages;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {

        public const string ACTION_SERVICE_MESSAGE = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
        public const string ACTION_EXTRA_MESSAGE = "Mk_Taxi.ACTION_EXTRA_MESSAGE";



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
			presenter.Show(new MvxShowViewModelRequest(type, null, false, MvxRequestedBy.UserAction));
		}

		public Task ShowMessage(string title, string message)
		{
			var ownerId = Guid.NewGuid().ToString();
			var dispatcher = TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
			var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();

			dispatcher.RequestMainThreadAction(() =>{
				var i = new Intent(Context, typeof(AlertDialogActivity));
				i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				i.PutExtra("Title", title);
				i.PutExtra("Message", message);
				i.PutExtra("OwnerId", ownerId );
				Context.StartActivity(i); 
			});

			var tcs = new TaskCompletionSource<object>();
			TinyMessageSubscriptionToken token = null;
			token = messengerHub.Subscribe<ActivityCompleted>(a =>
				{
					tcs.TrySetResult(null);
					messengerHub.Unsubscribe<ActivityCompleted>( token );
					token.Dispose();
				}, a => a.OwnerId == ownerId );

			return tcs.Task; 
		}
        
        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof(AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("OwnerId", ownerId );

            TinyMessageSubscriptionToken token = null;
            token = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<ActivityCompleted>( a=>
                        {
                                if ( a.Content == positiveButtonTitle )
                                {
                                    positiveAction();
                                }
                                else if ( a.Content == negativeButtonTitle )
                                {
                                    negativeAction();
                                }
                                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<ActivityCompleted>( token );
                                token.Dispose();
                        }, a=>a.OwnerId == ownerId );            

            Context.StartActivity(i); 
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof(AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("NeutralButtonTitle", neutralButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
            token = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<ActivityCompleted>(a =>
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
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<ActivityCompleted>(token);
                token.Dispose();
            }, a => a.OwnerId == ownerId);

            Context.StartActivity(i);
        }

        public void ShowMessage (string title, string message, List<KeyValuePair<string,Action>> additionalButton)
		{
			throw new NotImplementedException();
		}

        public void ShowMessage(string title, string message, Action additionalAction)
        {            
			var ownerId = Guid.NewGuid().ToString();
			var i = new Intent(Context, typeof(AlertDialogActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
			i.PutExtra("Title", title);
			i.PutExtra("Message", message);
			
			i.PutExtra("NeutralButtonTitle", "OK");
			i.PutExtra("OwnerId", ownerId);
			
			TinyMessageSubscriptionToken token = null;
			token = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<ActivityCompleted>(a =>
			{				
				additionalAction();				
				TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<ActivityCompleted>(token);
				token.Dispose();
			}, a => a.OwnerId == ownerId);
			
			Context.StartActivity(i);
        }

		Stack<ProgressDialog> progressDialogs = new Stack<ProgressDialog>();

        public void ShowProgress (bool show)
		{
			TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher.RequestMainThreadAction(() =>{

				var activity = TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

				if(show)
				{ 		
					var progress = new ProgressDialog(activity);
					progressDialogs.Push(progress);
					progress.SetTitle(string.Empty);
					progress.SetMessage(activity.GetString(Resource.String.LoadingMessage));
					progress.Show();

				}else{
					if(progressDialogs.Any())
					{
						var progressPrevious = progressDialogs.Pop();
						if(progressPrevious != null
						   && progressPrevious.IsShowing)
						{
							try{
								progressPrevious.Dismiss();
							}catch{} // on peut avoir une exception ici si activity est plus présente, pas grave
						}
					}
				}
			});
        }

		public IDisposable ShowProgress()
		{
			ShowProgress (true);
			return Disposable.Create (() => ShowProgress(false));
		}

        public void ShowToast(string message, ToastDuration duration )
        {
			TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher.RequestMainThreadAction(() =>{
	            Toast toast = Toast.MakeText(Context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
	            toast.Show();
			});
        }

		public void ShowDialog<T> (string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onResult)
		{
			var messenger = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
			var list = items.ToArray();
			if (displayNameSelector == null) {
				displayNameSelector = x => x.ToString ();
			}
			if (onResult == null) {
				onResult = result => {};
			}

			string[] displayList = list.Select(displayNameSelector).ToArray();


			var ownerId = Guid.NewGuid().ToString();
			var i = new Intent(Context, typeof(SelectItemDialogActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
			i.PutExtra("Title", title);
			i.PutExtra("Items", displayList);
			i.PutExtra("OwnerId", ownerId );
			TinyMessageSubscriptionToken token = null;
			token = messenger.Subscribe<SubNavigationResultMessage<int>>(msg =>
			                                                                    {
				if (token != null)
					messenger.Unsubscribe<SubNavigationResultMessage<int>>(token);

				onResult(list[msg.Result]);
			},
			msg => msg.MessageId == ownerId);
			Context.StartActivity(i); 
		}

        public void ShowEditTextDialog(string title, string message, string positiveButtonTitle, Action<string> positiveAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(Context, typeof(EditTextDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);
            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
            token = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<ActivityCompleted>(a =>
            {
                positiveAction(a.Content);
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<ActivityCompleted>(token);
                token.Dispose();
            }, a => a.OwnerId == ownerId);
            Context.StartActivity(i);
        }
    }
}