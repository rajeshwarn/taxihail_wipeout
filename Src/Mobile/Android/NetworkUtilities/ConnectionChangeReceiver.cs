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
using Android.Util;

//using System;
//using System.Globalization;
//using System.Net;
//using apcurium.MK.Common.Diagnostic;
//using ServiceStack.ServiceClient.Web;
//using Android.Content;
//using apcurium.MK.Booking.Mobile.Infrastructure;
//using TinyIoC;
using Android.App;
using Android.Net;



namespace MK.Booking.Mobile.Client.Android
{
    public class ConnectionChangeBroadcastReceiver : BroadcastReceiver
    {
        //const string TAG = "GCMBroadcastReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.CONNECTIVITY_SERVICE);
            NetworkInfo activeNetInfo = connectivityManager.getActiveNetworkInfo();
            NetworkInfo mobNetInfo = connectivityManager.getNetworkInfo(ConnectivityManager.TYPE_MOBILE);
            //if (activeNetInfo != null)
            //{
            //    Toast.makeText(context, "Active Network Type : " + activeNetInfo.getTypeName(), Toast.LENGTH_SHORT).show();
            //}
            //if (mobNetInfo != null)
            //{
            //    Toast.makeText(context, "Mobile Network Type : " + mobNetInfo.getTypeName(), Toast.LENGTH_SHORT).show();
            //}
            
            //Log.Verbose(TAG, "OnReceive: " + intent.Action);
            //var className = context.PackageName + GCMConstants.DEFAULT_INTENT_SERVICE_CLASS_NAME;

            //Log.Verbose(TAG, "GCM IntentService Class: " + className);

            //GCMBaseIntentService.RunIntentInService(context, intent, typeof(TIntentService));
            //SetResult(Result.Ok, null, null);
        }
    }
}