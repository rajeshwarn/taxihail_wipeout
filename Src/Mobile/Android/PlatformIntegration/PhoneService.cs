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
using Java.Util;
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
        public void AddEventToCalendarAndReminder(string title, string addInfo, string place, DateTime startDate)
        {

            //Calendar cal = Calendar.GetInstance();
            Intent intent = new Intent(Intent.ActionEdit);
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            intent.SetType("vnd.android.cursor.item/event");
            intent.PutExtra("beginTime", startDate.Ticks);
            intent.PutExtra("endTime", startDate.Ticks + 60 * 60 * 1000);
            intent.PutExtra("title", title);
            intent.PutExtra("description", addInfo);
            intent.PutExtra("eventLocation", place);
            Context.StartActivity(intent);

            String eventUriString = "content://com.android.calendar/events";
            ContentValues eventValues = new ContentValues();

            eventValues.Put("calendar_id", 1); // id, We need to choose from
                                                // our mobile for primary
                                                // its 1
            eventValues.Put("title", title);
            eventValues.Put("description", addInfo);
            eventValues.Put("eventLocation", place);
            long endDate = startDate.Ticks + 1000 * 60 * 60; // For next 1hr
            eventValues.Put("dtstart", startDate.Ticks);
            eventValues.Put("dtend", endDate);

            // values.put("allDay", 1); //If it is bithday alarm or such
            // kind (which should remind me for whole day) 0 for false, 1
            // for true
            eventValues.Put("eventStatus", "1"); // This information is
            // sufficient for most
            // entries tentative (0),
            // confirmed (1) or canceled
            // (2):
            eventValues.Put("visibility", 3); // visibility to default (0),
                                                // confidential (1), private
                                                // (2), or public (3):
            eventValues.Put("transparency", 0); // You can control whether
                                                // an event consumes time
                                                // opaque (0) or transparent
                                                // (1).
            eventValues.Put("hasAlarm", 0); // 0 for false, 1 for true

    Android.Net.Uri eventUri = Context.ApplicationContext.ContentResolver.Insert(Android.Net.Uri.Parse(eventUriString), eventValues);
    long eventID = long.Parse(eventUri.LastPathSegment);
   // if (needReminder) {
        /***************** Event: Reminder(with alert) Adding reminder to event *******************/

        String reminderUriString = "content://com.android.calendar/reminders";

        ContentValues reminderValues = new ContentValues();

        reminderValues.Put("event_id", eventID);
        reminderValues.Put("minutes", 120); // Default value of the
                                            // system. Minutes is a
                                            // integer
        reminderValues.Put("method", 1); // Alert Methods: Default(0),
                                            // Alert(1), Email(2),
                                            // SMS(3)

        Android.Net.Uri reminderUri = Context.ApplicationContext.ContentResolver.Insert(Android.Net.Uri.Parse(reminderUriString), reminderValues);
   // }
        }

		#endregion
    }
}