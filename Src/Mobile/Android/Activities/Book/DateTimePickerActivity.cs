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
using Java.Lang;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "@string/DateTimePickerPickTitle", Theme = "@android:style/Theme.Dialog")]
    public class DateTimePickerActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.DateTimePicker);

            long dateTimeticks = this.Intent.GetLongExtra("SelectedDate", 0);
            DateTime selected = dateTimeticks > 0 ? new DateTime(dateTimeticks) :DateTime.Now;            

            FindViewById<DatePicker>(Resource.Id.datePickerCtl).UpdateDate(selected.Year, selected.Month - 1, selected.Day);
            FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentHour = new Integer(selected.Hour);
            FindViewById<TimePicker>(Resource.Id.timePickerCtl).CurrentMinute = new Integer(selected.Minute);
            

            FindViewById<TimePicker>(Resource.Id.timePickerCtl).SetIs24HourView(Java.Lang.Boolean.True);

            FindViewById<Button>(Resource.Id.DoneButton).Click += DoneOnClick;
            FindViewById<Button>(Resource.Id.NowButton).Click += TimeOnClick;
        }

        private void TimeOnClick(object sender, EventArgs e)
        {
            var resultIntent = new Intent();
            resultIntent.SetFlags(ActivityFlags.ForwardResult);
            resultIntent.PutExtra("ResultSelectedDate", 0);
            SetResult(Result.Ok, resultIntent);
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
                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ForwardResult);
                intent.PutExtra("ResultSelectedDate", result.Ticks);
                SetResult(Result.Ok, intent);
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