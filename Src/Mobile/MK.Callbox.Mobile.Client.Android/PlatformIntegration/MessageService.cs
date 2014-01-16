using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Callbox.Mobile.Client.Activities;
using apcurium.MK.Callbox.Mobile.Client.Messages;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.Droid;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {
        public const string ACTION_SERVICE_MESSAGE = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
        public const string ACTION_EXTRA_MESSAGE = "Mk_Taxi.ACTION_EXTRA_MESSAGE";

		readonly Context _context;
		readonly ITinyMessengerHub _messengerHub;
		readonly IMvxViewDispatcher _viewDispatcher;
		readonly IMvxAndroidCurrentTopActivity _topActivity;

		public MessageService(IMvxAndroidGlobals globals,
			ITinyMessengerHub messengerHub,
			IMvxViewDispatcher viewDispatcher,
			IMvxAndroidCurrentTopActivity topActivity)
        {
			_topActivity = topActivity;
			_viewDispatcher = viewDispatcher;
			_messengerHub = messengerHub;
			_context = globals.ApplicationContext;
        }


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

			_viewDispatcher.RequestMainThreadAction(() =>{
				var i = new Intent(_context, typeof(AlertDialogActivity));
				i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				i.PutExtra("Title", title);
				i.PutExtra("Message", message);
				i.PutExtra("OwnerId", ownerId );
				_context.StartActivity(i); 
			});

			var tcs = new TaskCompletionSource<object>();
			TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>(a =>
				{
					tcs.TrySetResult(null);
					_messengerHub.Unsubscribe<ActivityCompleted>( token );
					token.Dispose();
				}, a => a.OwnerId == ownerId );

			return tcs.Task; 
		}
        
        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(_context, typeof(AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("OwnerId", ownerId );

            TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>( a=>
                        {
                                if ( a.Content == positiveButtonTitle )
                                {
                                    positiveAction();
                                }
                                else if ( a.Content == negativeButtonTitle )
                                {
                                    negativeAction();
                                }
								_messengerHub.Unsubscribe<ActivityCompleted>( token );
                                token.Dispose();
                        }, a=>a.OwnerId == ownerId );            

            _context.StartActivity(i); 
        }

        public void ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(_context, typeof(AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);

            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("NegativeButtonTitle", negativeButtonTitle);
            i.PutExtra("NeutralButtonTitle", neutralButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>(a =>
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
				_messengerHub.Unsubscribe<ActivityCompleted>(token);
                token.Dispose();
            }, a => a.OwnerId == ownerId);

            _context.StartActivity(i);
        }

        public void ShowMessage (string title, string message, List<KeyValuePair<string,Action>> additionalButton)
		{
			throw new NotImplementedException();
		}

        public void ShowMessage(string title, string message, Action additionalAction)
        {            
			var ownerId = Guid.NewGuid().ToString();
			var i = new Intent(_context, typeof(AlertDialogActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
			i.PutExtra("Title", title);
			i.PutExtra("Message", message);
			
			i.PutExtra("NeutralButtonTitle", "OK");
			i.PutExtra("OwnerId", ownerId);
			
			TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>(a =>
			{				
				additionalAction();				
				_messengerHub.Unsubscribe<ActivityCompleted>(token);
				token.Dispose();
			}, a => a.OwnerId == ownerId);
			
			_context.StartActivity(i);
        }

		Stack<ProgressDialog> progressDialogs = new Stack<ProgressDialog>();

        public void ShowProgress (bool show)
		{
			_viewDispatcher.RequestMainThreadAction(() =>{

				var activity = _topActivity.Activity;

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
			_viewDispatcher.RequestMainThreadAction(() =>{
	            Toast toast = Toast.MakeText(_context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
	            toast.Show();
			});
        }

		public void ShowDialog<T> (string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onResult)
		{
			var list = items.ToArray();
			if (displayNameSelector == null) {
				displayNameSelector = x => x.ToString ();
			}
			if (onResult == null) {
				onResult = result => {};
			}

			string[] displayList = list.Select(displayNameSelector).ToArray();


			var ownerId = Guid.NewGuid().ToString();
			var i = new Intent(_context, typeof(SelectItemDialogActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
			i.PutExtra("Title", title);
			i.PutExtra("Items", displayList);
			i.PutExtra("OwnerId", ownerId );
			TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<SubNavigationResultMessage<int>>(msg =>
			                                                                    {
				if (token != null)
				{
					_messengerHub.Unsubscribe<SubNavigationResultMessage<int>>(token);
				}

				onResult(list[msg.Result]);
			},
			msg => msg.MessageId == ownerId);
			_context.StartActivity(i); 
		}

        public void ShowEditTextDialog(string title, string message, string positiveButtonTitle, Action<string> positiveAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(_context, typeof(EditTextDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);
            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                positiveAction(a.Content);
				_messengerHub.Unsubscribe<ActivityCompleted>(token);
                token.Dispose();
            }, a => a.OwnerId == ownerId);
            _context.StartActivity(i);
        }
    }
}