using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using TaxiMobile.Activities.Setting;
using TaxiMobile.Helpers;
using TaxiMobile.Models;
using TaxiMobileApp;
using apcurium.Framework.Extensions;

namespace TaxiMobile.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait )]
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

            var serializedModel = Intent.GetStringExtra("BookingModel");
            var model = SerializerHelper.DeserializeObject<BookingModel>(serializedModel);
            BookingInfo = model.Data;
            var currentSettings = AppContext.Current.LoggedUser.DefaultSettings;
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((data != null) && (data.Extras != null))
            {
                var serializedSettings = data.GetStringExtra("BookingSettings");
                var bookingSettings = SerializerHelper.DeserializeObject<BookingSetting>(serializedSettings);
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
            FindViewById<TextView>(Resource.Id.OriginTxt).Text = BookingInfo.PickupLocation.Address;
            FindViewById<TextView>(Resource.Id.AptRingCode).Text = FormatAptRingCode(BookingInfo.PickupLocation.Apartment, BookingInfo.PickupLocation.RingCode);
            FindViewById<TextView>(Resource.Id.DestinationTxt).Text = BookingInfo.DestinationLocation.Address.IsNullOrEmpty() ? Resources.GetString(Resource.String.ConfirmDestinationNotSpecified) : BookingInfo.DestinationLocation.Address;
            FindViewById<TextView>(Resource.Id.DateTimeTxt).Text = FormatDateTime(BookingInfo.PickupDate, BookingInfo.PickupDate);
            FindViewById<TextView>(Resource.Id.NameTxt).Text = BookingInfo.Settings.Name;
            FindViewById<TextView>(Resource.Id.PhoneTxt).Text = BookingInfo.Settings.Phone;
            FindViewById<TextView>(Resource.Id.PassengersTxt).Text = BookingInfo.Settings.Passengers == 0 ? AppContext.Current.LoggedUser.DefaultSettings.Passengers.ToString() : BookingInfo.Settings.Passengers.ToString();
            FindViewById<TextView>(Resource.Id.VehiculeTypeTxt).Text = BookingInfo.Settings.VehicleTypeName;
            FindViewById<TextView>(Resource.Id.CampanyTxt).Text = BookingInfo.Settings.CompanyName;
            FindViewById<TextView>(Resource.Id.ChargeTypeTxt).Text = BookingInfo.Settings.ChargeTypeName;

            var price = BookingInfo.GetPrice(BookingInfo.GetDistance());
            if (price.HasValue)
            {
                FindViewById<TextView>(Resource.Id.ApproxPriceTxt).Text = String.Format("{0:C}", price.Value);
            }
            else
            {
                FindViewById<TextView>(Resource.Id.ApproxPriceTxt).Text = Resources.GetString(Resource.String.NotAvailable);
            }
        }


    }
}
