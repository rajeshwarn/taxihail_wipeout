using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class BookDetailActivity : Activity
    {
        private BookingInfoData _bookingInfo;
        public BookingInfoData BookingInfo
        {
            get { return _bookingInfo; }
            private set { _bookingInfo = value; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.BookingDetail);

            var serialized = Intent.GetStringExtra("BookingInfoData");
            var data = SerializerHelper.DeserializeObject<BookingInfoData>(serialized);
            BookingInfo = data;
            var currentSettings = AppContext.Current.LoggedUser.Settings;
            BookingInfo.Settings = currentSettings;

            UpdateDisplay();

            FindViewById<Button>(Resource.Id.ConfirmBtn).Click += new EventHandler(Confirm_Click);
            FindViewById<Button>(Resource.Id.CancelBtn).Click += new EventHandler(Cancel_Click);
            FindViewById<Button>(Resource.Id.EditBtn).Click += new EventHandler(Edit_Click);
        }

        void Edit_Click(object sender, EventArgs e)
        {
            var i = new Intent().SetClass(this, typeof(RideSettingsActivity));
            i.PutExtra("BookingSettings", BookingInfo.Settings.Serialize());
            StartActivityForResult(i, 2);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((data != null) && (data.Extras != null))
            {
                var serializedSettings = data.GetStringExtra("BookingSettings");
                var bookingSettings = SerializerHelper.DeserializeObject<BookingSettings>(serializedSettings);
                BookingInfo.Settings = bookingSettings;
                UpdateDisplay();
            }
        }

        void Confirm_Click(object sender, EventArgs e)
        {

            var value = BookingInfo.Serialize();

            Intent intent = new Intent();
            intent.SetFlags(ActivityFlags.ForwardResult);
            intent.PutExtra("ConfirmedBookingInfo", value);
            SetResult(Result.Ok, intent);

            Finish();
        }

        void Cancel_Click(object sender, EventArgs e)
        {
            Finish();
        }


        private string FormatDateTime(DateTime? pickupDate, DateTime? pickupTime)
        {
            string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString() : Resources.GetString(Resource.String.DateToday);
            result += @" / ";
            result += pickupTime.HasValue && (pickupTime.Value.Hour != 0 && pickupTime.Value.Minute != 0) ? pickupTime.Value.ToShortTimeString() : Resources.GetString(Resource.String.TimeNow);
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {

            string result = apt.HasValue() ? apt : Resources.GetString(Resource.String.ConfirmNoApt);
            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.GetString(Resource.String.ConfirmNoRingCode);
            return result;
        }


        private void UpdateDisplay()
        {
            FindViewById<TextView>(Resource.Id.OriginTxt).Text = BookingInfo.PickupLocation.FullAddress;
            FindViewById<TextView>(Resource.Id.AptRingCode).Text = FormatAptRingCode(BookingInfo.PickupLocation.Apartment, BookingInfo.PickupLocation.RingCode);
            FindViewById<TextView>(Resource.Id.DestinationTxt).Text = BookingInfo.DestinationLocation.FullAddress.IsNullOrEmpty() ? Resources.GetString(Resource.String.ConfirmDestinationNotSpecified) : BookingInfo.DestinationLocation.FullAddress;
            FindViewById<TextView>(Resource.Id.DateTimeTxt).Text = FormatDateTime(BookingInfo.PickupDate, BookingInfo.PickupDate);
            FindViewById<TextView>(Resource.Id.NameTxt).Text = BookingInfo.Settings.FirstName;
            FindViewById<TextView>(Resource.Id.PhoneTxt).Text = BookingInfo.Settings.Phone;
            FindViewById<TextView>(Resource.Id.PassengersTxt).Text = BookingInfo.Settings.Passengers == 0 ? AppContext.Current.LoggedUser.Settings.Passengers.ToString() : BookingInfo.Settings.Passengers.ToString();
            
            
            //TODO:Fix this
            //FindViewById<TextView>(Resource.Id.VehiculeTypeTxt).Text = BookingInfo.Settings.VehicleTypeName;
            //FindViewById<TextView>(Resource.Id.CampanyTxt).Text = BookingInfo.Settings.CompanyName;
            //FindViewById<TextView>(Resource.Id.ChargeTypeTxt).Text = BookingInfo.Settings.ChargeTypeName;

            var direction = BookingInfo.GetDirectionInfo();

            //var price = BookingInfo.GetPrice(BookingInfo.GetDistance());
            if (direction.Price.HasValue)
            {
                FindViewById<TextView>(Resource.Id.ApproxPriceTxt).Text = String.Format("{0:C}", direction.Price.Value);
            }
            else
            {
                FindViewById<TextView>(Resource.Id.ApproxPriceTxt).Text = Resources.GetString(Resource.String.NotAvailable);
            }
        }


    }
}
