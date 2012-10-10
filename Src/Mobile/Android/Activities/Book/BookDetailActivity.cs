using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.ListViewCell;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class BookDetailActivity : BaseActivity
    {
        private CreateOrder _bookingInfo;
        public CreateOrder BookingInfo
        {
            get { return _bookingInfo; }
            private set { _bookingInfo = value; }
        }

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_BookingDetail);

            var serialized = Intent.GetStringExtra("BookingInfo");
            var data = SerializerHelper.DeserializeObject<CreateOrder>(serialized);
            BookingInfo = data;
            var currentSettings = AppContext.Current.LoggedUser.Settings;
            BookingInfo.Settings = currentSettings;

            UpdateDisplay();

            FindViewById<Button>(Resource.Id.ConfirmBtn).Click += new EventHandler(Confirm_Click);
            FindViewById<Button>(Resource.Id.CancelBtn).Click += new EventHandler(Cancel_Click);
            FindViewById<Button>(Resource.Id.EditBtn).Click += new EventHandler(Edit_Click);
            if (TinyIoCContainer.Current.Resolve<ICacheService>().Get<string>("WarningEstimateDontShow").IsNullOrEmpty() && _bookingInfo.DropOffAddress.HasValidCoordinate())
            {
                ShowAlertDialog();
            }
            //TinyIoCContainer.Current.Resolve<ICacheService>().ClearAll();
            if (TinyIoCContainer.Current.Resolve<IAppSettings>().CanChooseProvider)
            {
                if(BookingInfo.Settings.ProviderId ==null)
                {
                    ShowChooseProviderDialog();
                }
            }
        }

        public void ShowChooseProviderDialog()
        {
            var service = TinyIoCContainer.Current.Resolve<IAccountService>();
            var companyList = service.GetCompaniesList();
            var hashmap = new List<int>();

            for(int i=0; i< companyList.Count();i++)
            {
                hashmap.Add(companyList.ElementAt(i).Id);
            }

            string[] list = companyList.Select(c => c.Display).ToArray();
            var chooseProviderDialog = new AlertDialog.Builder(this);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SelectDialogItem, list);
            chooseProviderDialog.SetTitle("Pick a provider");
            chooseProviderDialog.SetAdapter(adapter, (sender, args) => 
            {
                BookingInfo.Settings.ProviderId = hashmap[(int)args.Which];
                TinyIoCContainer.Current.Resolve<IAccountService>().UpdateBookingSettings(BookingInfo.Settings);
                FindViewById<TextView>(Resource.Id.CompanyTxt).Text = companyList.ElementAt(args.Which).Display;
                chooseProviderDialog.Dispose();
            });
            chooseProviderDialog.Show();
        }

        public void ShowAlertDialog()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle(Resources.GetString(Resource.String.WarningEstimateTitle));
            alert.SetMessage(Resources.GetString(Resource.String.WarningEstimate));

            alert.SetPositiveButton("Ok", (s, e) => alert.Dispose());

            alert.SetNegativeButton(Resource.String.WarningEstimateDontShow, (s, e) =>
                {
                    TinyIoCContainer.Current.Resolve<ICacheService>().Set("WarningEstimateDontShow", "yes");
                    alert.Dispose();
                });

            alert.Show();
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
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new OrderConfirmed(this, BookingInfo));                        

            Finish();
        }

        void Cancel_Click(object sender, EventArgs e)
        {
            Finish();
        }


        private string FormatDateTime(DateTime? pickupDate )
        {
            string format = "{0:dddd, MMM d}, {0:h:mm tt}";
            string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.GetString(Resource.String.TimeNow);
            //result += @" / ";
            //result += pickupTime.HasValue && (pickupTime.Value.Hour != 0 && pickupTime.Value.Minute != 0) ? pickupTime.Value.ToShortTimeString() : Resources.GetString(Resource.String.TimeNow);
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

            var service = TinyIoCContainer.Current.Resolve<IAccountService>();
            
            var companies = service.GetCompaniesList();
            //TODO : data depends on company selected            


            FindViewById<TextView>(Resource.Id.OriginTxt).Text = BookingInfo.PickupAddress.FullAddress;
            FindViewById<TextView>(Resource.Id.AptRingCode).Text = FormatAptRingCode(BookingInfo.PickupAddress.Apartment, BookingInfo.PickupAddress.RingCode);
            FindViewById<TextView>(Resource.Id.DestinationTxt).Text = BookingInfo.DropOffAddress.FullAddress.IsNullOrEmpty() ? Resources.GetString(Resource.String.ConfirmDestinationNotSpecified) : BookingInfo.DropOffAddress.FullAddress;
            FindViewById<TextView>(Resource.Id.DateTimeTxt).Text = FormatDateTime(BookingInfo.PickupDate );
            FindViewById<TextView>(Resource.Id.NameTxt).Text = BookingInfo.Settings.Name;
            FindViewById<TextView>(Resource.Id.PhoneTxt).Text = BookingInfo.Settings.Phone;
            FindViewById<TextView>(Resource.Id.PassengersTxt).Text = BookingInfo.Settings.Passengers == 0 ? AppContext.Current.LoggedUser.Settings.Passengers.ToString() : BookingInfo.Settings.Passengers.ToString();

            var model = new RideSettingsModel(BookingInfo.Settings, companies, service.GetVehiclesList(), service.GetPaymentsList());
            //TODO:Fix this
            FindViewById<TextView>(Resource.Id.VehiculeTypeTxt).Text = model.VehicleTypeName;
            FindViewById<TextView>(Resource.Id.CompanyTxt).Text = model.ProviderName;
            FindViewById<TextView>(Resource.Id.ChargeTypeTxt).Text = model.ChargeTypeName;

            FindViewById<TextView>(Resource.Id.ApproxPriceTxt).Text = TinyIoCContainer.Current.Resolve<IBookingService>().GetFareEstimateDisplay(BookingInfo, null, "NotAvailable");  
        }


    }
}
