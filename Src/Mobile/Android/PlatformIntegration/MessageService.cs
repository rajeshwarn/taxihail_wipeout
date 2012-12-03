using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
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



        public void ShowMessage(string title, string message)
        {
            
            var i = new Intent(Context, typeof(AlertDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);
            Context.StartActivity(i); 
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

        public void ShowProgress(bool show)
        {
            /*TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher.RequestMainThreadAction(
                () =>
                    {
                        new ProgressDialog(Context).Show();.Show(Context, "", "LoadingMessage", true,
                                                              false);
                       _progressDialog.Show();
                    }
                );*/
			/* var i = new Intent(Context, typeof(ShowDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags..ReorderToFront);
            i.PutExtra("Show", show.ToString());
            Context.StartActivity(i);
            Context.ac*/
        }          

        public void ShowProgress(bool show, Action cancel)
        {
            var i = new Intent(Context, typeof(ShowDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Show", show.ToString());
        }

        public void ShowToast(string message, ToastDuration duration )
        {
            Toast toast = Toast.MakeText(Context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
            toast.Show();
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
    }
}