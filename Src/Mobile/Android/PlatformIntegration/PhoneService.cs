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
using System.IO;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PhoneService : IPhoneService
    {
		public PhoneService(Context context)
        {
            Context = context;
        }
        public Context Context { get; set; }

		#region IPhoneService implementation
        public void Call(string phoneNumber)
        {
            Intent callIntent = new Intent(Intent.ActionCall);
			callIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            callIntent.SetData(Android.Net.Uri.Parse("tel:" + phoneNumber));
            Context.StartActivity(callIntent);
        }

		public void SendFeedbackErrorLog (string errorLogPath, string supportEmail, string subject)
		{
			Intent emailIntent = new Intent(Intent.ActionSend);

			var resource = TinyIoC.TinyIoCContainer.Current.Resolve<IAppResource>();
			
			emailIntent.SetType("message/rfc822");
			emailIntent.PutExtra(Intent.ExtraEmail, new String[] { supportEmail });
			emailIntent.PutExtra(Intent.ExtraCc, new String[] { AppContext.Current.LoggedInEmail });
			emailIntent.PutExtra(Intent.ExtraSubject, subject);
			
			emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(@"file:///" + LoggerImpl.LogFilename));
			if (File.Exists(errorLogPath))
			{
				emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(errorLogPath));
			}
			try
			{
				var intent = Intent.CreateChooser(emailIntent, resource.GetString("SendEmail"));
				intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				Context.StartActivity(intent);
				LoggerImpl.FlushNextWrite();
			}
			catch (Android.Content.ActivityNotFoundException ex)
			{

				Toast.MakeText(Context, resource.GetString("NoMailClient"), ToastLength.Short).Show();
			}
		}

		#endregion
    }
}