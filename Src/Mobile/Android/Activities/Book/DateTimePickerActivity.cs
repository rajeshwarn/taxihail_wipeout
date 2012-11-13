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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_DateTimePicker);

            long dateTimeticks = this.Intent.GetLongExtra("SelectedDate", 0);
            DateTime selected = dateTimeticks > 0 ? new DateTime(dateTimeticks) :DateTime.Now;

            var timePicker = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            timePicker.SetIs24HourView(Java.Lang.Boolean.True);
            timePicker.CurrentHour = new Integer(selected.Hour);
            timePicker.CurrentMinute = new Integer(selected.Minute);
            timePicker.SetIs24HourView(Java.Lang.Boolean.False);

            FindViewById<DatePicker>(Resource.Id.datePickerCtl).UpdateDate(selected.Year, selected.Month - 1, selected.Day);
            FindViewById<Button>(Resource.Id.DoneButton).Click += DoneOnClick;
            FindViewById<Button>(Resource.Id.NowButton).Click += TimeOnClick;
        }

        private void TimeOnClick(object sender, EventArgs e)
        {
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this,null));
            Finish();
        }

        private void DoneOnClick(object sender, EventArgs eventArgs)
        {
            var dcl = FindViewById<DatePicker>(Resource.Id.datePickerCtl);
            var tcl = FindViewById<TimePicker>(Resource.Id.timePickerCtl);
            dcl.ClearFocus();
            tcl.ClearFocus();
            
            var result = new DateTime(dcl.Year, dcl.Month + 1, dcl.DayOfMonth, tcl.CurrentHour.IntValue(), tcl.CurrentMinute.IntValue(),0 );

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
    }
}