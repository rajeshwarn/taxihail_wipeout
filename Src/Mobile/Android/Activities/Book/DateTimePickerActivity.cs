using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Java.Lang;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "@string/DateTimePickerPickTitle", Theme = "@android:style/Theme.Dialog")]
    public class DateTimePickerActivity : Activity
    {

        private static int TIME_PICKER_INTERVAL = 5;

		private int _lastMinutes;
        private bool _ignoreEvent = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_DateTimePicker);

            long dateTimeticks = this.Intent.GetLongExtra("SelectedDate", 0);
            DateTime selected = dateTimeticks > 0 ? new DateTime(dateTimeticks) : GetNowWithInterval( );

            var timePicker = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            timePicker.SetIs24HourView(Java.Lang.Boolean.True);
            timePicker.CurrentHour = new Integer(selected.Hour);
            timePicker.CurrentMinute = new Integer(selected.Minute);
            timePicker.SetIs24HourView(Java.Lang.Boolean.False);  
            
			_lastMinutes = selected.Minute == 0 ? 60 : selected.Minute;

			FindViewById<TimePicker>(Resource.Id.timePickerCtl).TimeChanged += TimeOnTimeChanged;
            FindViewById<DatePicker>(Resource.Id.datePickerCtl).UpdateDate(selected.Year, selected.Month - 1, selected.Day);
            FindViewById<Button>(Resource.Id.DoneButton).Click += DoneOnClick;
            //FindViewById<Button>(Resource.Id.NowButton).Click += TimeOnClick;

            var useAmPm = this.Intent.GetBooleanExtra("UseAmPmFormat", true );

            if ( !useAmPm )
            {
                var h = timePicker.CurrentHour.IntValue ();               
                timePicker.SetIs24HourView(Java.Lang.Boolean.True);                                       
                timePicker.CurrentHour = new Integer(h);
            }


        }

        private DateTime GetNowWithInterval()
        {
            var now = DateTime.Now;
            int i = GetNextIntervalMinutes(now.Minute);
            if ( i < now.Minute )
            {
                return new DateTime(now.Year, now.Month, now.Day, now.Hour + 1,  0, 0 );
            }
            return new DateTime(now.Year, now.Month, now.Day, now.Hour,  i, 0 );
        }
        private void TimeOnClick(object sender, EventArgs e)
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, null));
            Finish();
        }

        private void DoneOnClick(object sender, EventArgs eventArgs)
        {
            var dcl = FindViewById<DatePicker>(Resource.Id.datePickerCtl);
            var tcl = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            dcl.ClearFocus();
            tcl.ClearFocus();
            
            var result = new DateTime(dcl.Year, dcl.Month + 1, dcl.DayOfMonth, tcl.CurrentHour.IntValue(), tcl.CurrentMinute.IntValue(), 0);

            if (result > DateTime.Now)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, result));                                        
                Finish();
            }
            else
            {
                this.ShowAlert(Resource.String.DateTimePickerPickInvalidDateTitle,
                               Resource.String.DateTimePickerPickInvalidDateMessage);
            }
        }

        private void TimeOnTimeChanged(object sender, TimePicker.TimeChangedEventArgs  e)
        {
            if (_ignoreEvent)
                return;
            


			bool goingUp =  e.Minute > _lastMinutes;

            int i = GetNextIntervalMinutes(e.Minute);

			_lastMinutes = i == 0 ? 60 : i;

            _ignoreEvent = true;
            
			if ( goingUp && (i < e.Minute) )
            {

                var h = FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentHour.IntValue();
                h ++;
                if (h > 23)
                {
                    h = 0;
                }
                FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentHour = new Integer(h);
                
            }
            FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentMinute = new Integer(i);
            _ignoreEvent = false;
        }
        
        private int GetNextIntervalMinutes(int minute)
        {
            if (minute % TIME_PICKER_INTERVAL != 0)
            {
                int minuteFloor = minute - (minute % TIME_PICKER_INTERVAL);
                minute = minuteFloor + (minute == minuteFloor + 1 ? TIME_PICKER_INTERVAL : 0);
                if (minute == 60)
                {
                    minute = 0;
                }
                
            }
            return minute;
        }

        public override void OnBackPressed()
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, null)); 
            base.OnBackPressed();
        }
    }
}