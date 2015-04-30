using System;
using apcurium.MK.Booking.Mobile.Client.Models;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "@string/HomeView_BookTaxi", Theme = "@style/BookATaxiDialog")]
    public class OrderBookingOptionsDialogActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			SetContentView(Resource.Layout.View_OrderBookingOptionsDialog);

            var btnBookNow = FindViewById<Button>(Resource.Id.btnBookNow);
            var btnBookLater = FindViewById<Button>(Resource.Id.btnBookLater);
            var btnCancel = FindViewById<Button>(Resource.Id.btnCancel);

            btnBookNow.Click += BtnBookNow_Click;
            btnBookLater.Click += BtnBookLater_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(object sender, EventArgs eventArgs)
        {
            SetResult(Result.Canceled, new Intent());
            Finish();
        }

        private void BtnBookLater_Click(object sender, EventArgs eventArgs)
        {
            SendResult(BookATaxiEnum.BookLater);
        }

        private void BtnBookNow_Click(object sender, EventArgs e)
        {
            SendResult(BookATaxiEnum.BookNow);   
        }

        private void SendResult(BookATaxiEnum bookATaxiResult)
        {
            var result = new Intent();
            result.PutExtra("BookATaxiResult", (int)bookATaxiResult);
            SetResult(Result.Ok, result);
            Finish();
        }
    }
}