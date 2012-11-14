using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

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
        public void ShowMessage(string title, string message,  string additionnalActionButtonTitle, Action additionalAction)
        {            
        }

        public void ShowProgress(bool show)
        {         
        }

        public void ShowProgress(bool show, Action cancel)
        {
            
        }

        public void ShowToast(string message, ToastDuration duration )
        {
            Toast toast = Toast.MakeText(Context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
            toast.Show();
        }

    }
}