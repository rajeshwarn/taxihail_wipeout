using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Messages;
using Java.Lang;
using Boolean = Java.Lang.Boolean;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "@string/DateTimePickerPickTitle", Theme = "@android:style/Theme.Dialog")]
    public class DateTimePickerActivity : Activity
    {
	    private const int TIME_PICKER_INTERVAL = 5;

	    private bool _ignoreEvent;
        private int _lastMinutes;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_DateTimePicker);

            var dateTimeticks = Intent.GetLongExtra("SelectedDate", 0);
            var selected = dateTimeticks > 0 ? new DateTime(dateTimeticks) : GetNowWithInterval();

            var timePicker = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            timePicker.SetIs24HourView(Boolean.True);
            timePicker.CurrentHour = new Integer(selected.Hour);
            timePicker.CurrentMinute = new Integer(selected.Minute);
            timePicker.SetIs24HourView(Boolean.False);

            _lastMinutes = selected.Minute == 0 ? 60 : selected.Minute;

            FindViewById<TimePicker>(Resource.Id.timePickerCtl).TimeChanged += TimeOnTimeChanged;
            FindViewById<DatePicker>(Resource.Id.datePickerCtl).UpdateDate(selected.Year, selected.Month - 1, selected.Day);
            FindViewById<Button>(Resource.Id.DoneButton).Click += DoneOnClick;

            var useAmPm = Intent.GetBooleanExtra("UseAmPmFormat", true);

            if (!useAmPm)
            {
                var h = timePicker.CurrentHour.IntValue();
                timePicker.SetIs24HourView(Boolean.True);
                timePicker.CurrentHour = new Integer(h);
            }
        }

        private DateTime GetNowWithInterval()
        {
            var now = DateTime.Now;
            var nextIntervalMinutes = GetNextIntervalMinutes(now.Minute);
            return nextIntervalMinutes < now.Minute 
				? new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0) 
				: new DateTime(now.Year, now.Month, now.Day, now.Hour, nextIntervalMinutes, 0);
        }

        private void DoneOnClick(object sender, EventArgs eventArgs)
        {
            var dcl = FindViewById<DatePicker>(Resource.Id.datePickerCtl);
            var tcl = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            dcl.ClearFocus();
            tcl.ClearFocus();

            var dateTime = new DateTime(dcl.Year, dcl.Month + 1, dcl.DayOfMonth, tcl.CurrentHour.IntValue(), tcl.CurrentMinute.IntValue(), 0);

            if (dateTime > DateTime.Now)
            {
                var result = new Intent();              
                result.PutExtra("DateTimeResult", dateTime.Ticks);
                SetResult(Android.App.Result.Ok, result);
                Finish();
            }
            else
            {
                this.ShowAlert(Resource.String.DateTimePickerPickInvalidDateTitle, Resource.String.DateTimePickerPickInvalidDateMessage);
            }
        }

        public static int Result { get; set;}

        private void TimeOnTimeChanged(object sender, TimePicker.TimeChangedEventArgs e)
        {
            if (_ignoreEvent)
                return;


            var goingUp = e.Minute > _lastMinutes;

            var nextIntervalMinutes = GetNextIntervalMinutes(e.Minute);

            _lastMinutes = nextIntervalMinutes == 0 ? 60 : nextIntervalMinutes;

            _ignoreEvent = true;

            if (goingUp && (nextIntervalMinutes < e.Minute))
            {
                var h = FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentHour.IntValue();
                h ++;
                if (h > 23)
                {
                    h = 0;
                }
                FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentHour = new Integer(h);
            }
            FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentMinute = new Integer(nextIntervalMinutes);
            _ignoreEvent = false;
        }

        private int GetNextIntervalMinutes(int minute)
        {
	        if (minute%TIME_PICKER_INTERVAL == 0)
	        {
		        return minute;
	        }
	        var minuteFloor = minute - (minute%TIME_PICKER_INTERVAL);
	        minute = minuteFloor + (minute == minuteFloor + 1 ? TIME_PICKER_INTERVAL : 0);
	        if (minute == 60)
	        {
		        minute = 0;
	        }
	        return minute;
        }

        public override void OnBackPressed()
        {
			this.Services().MessengerHub.Publish(new DateTimePicked(this, null));
            base.OnBackPressed();
        }
    }
}