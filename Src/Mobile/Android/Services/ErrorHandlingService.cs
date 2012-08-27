
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
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Services
{
	[Service (Label = "ErrorDisplayService")]			
	public class ErrorHandlingService : Service
	{
		private BroadcastReceiver _errorReceiver;
		public override void OnCreate ()
		{
			base.OnCreate ();
						
			IntentFilter filter = new IntentFilter(ErrorHandler.ACTION_EUC);
            filter.AddCategory(Intent.CategoryDefault);
            _errorReceiver = new ErrorBroadcastReceiver(this);
            RegisterReceiver(_errorReceiver, filter);
		}

		public override IBinder OnBind (Intent intent)
		{
			return null;
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			UnregisterReceiver( _errorReceiver );
		}
		public void DisplayError( Context context,string action )
		{
			string title = "";
			string message = "";


			switch( action )
			{
			case ErrorHandler.ACTION_EUC:
				title = Resources.GetString( Resource.String.UnAuthorizedCallTitle );
				message = Resources.GetString( Resource.String.UnAuthorizedCallMessage );
				break;
			}

			var i = new Intent( this, typeof(AlertDialogActivity) );
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront );
			i.PutExtra("Title", title );
			i.PutExtra("Message", message);
			StartActivity( i );
		}
	}

	[BroadcastReceiver]
    public class ErrorBroadcastReceiver : BroadcastReceiver
    {
		private ErrorHandlingService _service;

        public ErrorBroadcastReceiver()
        {
        }
        
		public ErrorBroadcastReceiver(ErrorHandlingService service)
        {
            _service = service;
        }

        public override void OnReceive(Context context, Intent intent)
        {
			_service.DisplayError( context, intent.Action );
        }
    }
}

