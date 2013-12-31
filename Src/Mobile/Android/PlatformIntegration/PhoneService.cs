using System;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Java.Util;
using TinyIoC;
using TimeZone = Java.Util.TimeZone;
using Uri = Android.Net.Uri;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
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
            var callIntent = new Intent(Intent.ActionCall);
            callIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            callIntent.SetData(Uri.Parse("tel:" + phoneNumber));
            Context.StartActivity(callIntent);
        }

        public void SendFeedbackErrorLog(string errorLogPath, string supportEmail, string subject)
        {
            var emailIntent = new Intent(Intent.ActionSend);

            var resource = TinyIoCContainer.Current.Resolve<IAppResource>();

            emailIntent.SetType("message/rfc822");
            emailIntent.PutExtra(Intent.ExtraEmail, new[] {supportEmail});
            emailIntent.PutExtra(Intent.ExtraSubject, subject);

            emailIntent.PutExtra(Intent.ExtraStream, Uri.Parse(@"file:///" + LoggerImpl.LogFilename));
            if (File.Exists(errorLogPath))
            {
                emailIntent.PutExtra(Intent.ExtraStream, Uri.Parse(errorLogPath));
            }
            try
            {
                var intent = Intent.CreateChooser(emailIntent, resource.GetString("SendEmail"));
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
                Context.StartActivity(intent);
                LoggerImpl.FlushNextWrite();
            }
            catch
            {
                Toast.MakeText(Context, resource.GetString("NoMailClient"), ToastLength.Short).Show();
            }
        }

        public void AddEventToCalendarAndReminder(string title, string addInfo, string place, DateTime startDate,
            DateTime alertDate)
        {
            try
            {
                string uriCalendars = "content://com.android.calendar/calendars";
                string eventUriString = "content://com.android.calendar/events";
                string reminderUriString = "content://com.android.calendar/reminders";

                var cursor = Context.ApplicationContext.ContentResolver.Query(Uri.Parse(uriCalendars),
                    new[] {"_id"}, null, null, null);
                cursor.MoveToFirst();
                var calIds = new int[cursor.Count];
                for (int i = 0; i < calIds.Length; i++)
                {
                    calIds[i] = cursor.GetInt(0);
                    cursor.MoveToNext();
                }

                cursor.Close();

                var eventValues = new ContentValues();
                eventValues.Put("calendar_id", calIds[0]);
                eventValues.Put("title", title);
                eventValues.Put("description", addInfo);
                eventValues.Put("eventLocation", place);

                eventValues.Put("dtstart", GetDateTimeMS(startDate));
                var endDate = startDate.AddHours(1); // For next 1hr
                eventValues.Put("dtend", GetDateTimeMS(endDate));
                eventValues.Put("eventTimezone", "UTC");
                eventValues.Put("eventEndTimezone", "UTC");

                Uri eventUri =
                    Context.ApplicationContext.ContentResolver.Insert(Uri.Parse(eventUriString), eventValues);
                long eventId = long.Parse(eventUri.LastPathSegment);


                var reminderValues = new ContentValues();

                reminderValues.Put("event_id", eventId);
                reminderValues.Put("minutes", 120);
                reminderValues.Put("method", 1);

                Context.ApplicationContext.ContentResolver.Insert(Uri.Parse(reminderUriString),
                    reminderValues);
            }
            catch (Exception e)
            {
                var logger = TinyIoCContainer.Current.Resolve<ILogger>();
                logger.LogError(e);
            }
        }

        public bool CanUseCalendarAPI()
        {
            return (int) Build.VERSION.SdkInt > 8;
        }

        private long GetDateTimeMS(DateTime date)
        {
            var c = Calendar.GetInstance(TimeZone.Default);

            c.Set(CalendarField.DayOfMonth, date.Day);
            c.Set(CalendarField.HourOfDay, date.Hour);
            c.Set(CalendarField.Minute, date.Minute);
            c.Set(CalendarField.Month, date.Month - 1);
            c.Set(CalendarField.Year, date.Year);

            return c.TimeInMillis;
        }
    }
}