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

        public void ShowToast(string message, ToastDuration duration )
        {
            Toast toast = Toast.MakeText(Context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
            toast.Show();
        }

         
    }
}