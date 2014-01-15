using System;
using Android.Content;
using Android.Widget;
using Java.Util;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.IO;
using apcurium.MK.Callbox.Mobile.Client.Diagnostic;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{
    public class PhoneService : IPhoneService
    {
		public PhoneService(Context context)
        {
            Context = context;
        }
        public Context Context { get; set; }


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

			var resource = TinyIoC.TinyIoCContainer.Current.Resolve<ILocalization>();
			
			emailIntent.SetType("message/rfc822");
			emailIntent.PutExtra(Intent.ExtraEmail, new String[] { supportEmail });
			emailIntent.PutExtra(Intent.ExtraSubject, subject);
			
			emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(@"file:///" + LoggerImpl.LogFilename));
			if (File.Exists(errorLogPath))
			{
				emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(errorLogPath));
			}
			try
			{
				var intent = Intent.CreateChooser(emailIntent, resource["SendEmail"]);                    
				intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				Context.StartActivity(intent);
				LoggerImpl.FlushNextWrite();
			}
			catch
			{
				Toast.MakeText(Context, resource["NoMailClient"], ToastLength.Short).Show();
			}
		}

        public void AddEventToCalendarAndReminder (string title, string addInfo, string place, DateTime startDate, DateTime alertDate)
		{
			string uriCalendars = "content://com.android.calendar/calendars";
			string eventUriString = "content://com.android.calendar/events";
			string reminderUriString = "content://com.android.calendar/reminders";

			var cursor = Context.ApplicationContext.ContentResolver.Query (Android.Net.Uri.Parse (uriCalendars), new String[] { "_id" }, null, null, null);
			cursor.MoveToFirst ();
			int[] CalIds = new int[cursor.Count];
			for (int i = 0; i < CalIds.Length; i++) {
				CalIds [i] = cursor.GetInt (0);
				cursor.MoveToNext ();
			}
	
			cursor.Close ();


			ContentValues eventValues = new ContentValues ();
    

			eventValues.Put ("calendar_id", CalIds [0]);
			eventValues.Put ("title", title);
			eventValues.Put ("description", addInfo);
			eventValues.Put ("eventLocation", place);
    
			eventValues.Put ("dtstart", GetDateTimeMS (startDate));
			var endDate = startDate.AddHours (1); // For next 1hr
			eventValues.Put ("dtend", GetDateTimeMS (endDate));
			eventValues.Put ("eventTimezone", "UTC");
			eventValues.Put ("eventEndTimezone", "UTC");        

			Android.Net.Uri eventUri = Context.ApplicationContext.ContentResolver.Insert (Android.Net.Uri.Parse (eventUriString), eventValues);
			long eventID = long.Parse (eventUri.LastPathSegment);



			ContentValues reminderValues = new ContentValues ();

			reminderValues.Put ("event_id", eventID);
			reminderValues.Put ("minutes", 120); 
			reminderValues.Put ("method", 1);
        }

		long GetDateTimeMS (DateTime date)
		{
			var c = Calendar.GetInstance (Java.Util.TimeZone.Default);
			
			c.Set (CalendarField.DayOfMonth, date.Day);
			c.Set (CalendarField.HourOfDay, date.Hour);
			c.Set (CalendarField.Minute, date.Minute);
			c.Set (CalendarField.Month, date.Month -1);
			c.Set (CalendarField.Year, date.Year);
			
			return c.TimeInMillis;
		}

		public bool CanUseCalendarAPI ()
		{
			return (int)Android.OS.Build .VERSION.SdkInt > 8;
		}
    }

}