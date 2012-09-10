
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class ConfirmationView : UIViewController
    {
        private BookTabView _parent;

        public event EventHandler Confirmed;
        public event EventHandler Canceled;
        public delegate void NoteChangedEventHandler(string note);

        public event NoteChangedEventHandler NoteChanged;

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public ConfirmationView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public ConfirmationView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public ConfirmationView(BookTabView parent) : base("ConfirmationView", null)
        {
            _parent = parent;
            Initialize();
        }

        void Initialize()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = true;

			lblRideSettings.TextColor = AppStyle.TitleTextColor;
			lblRideInfo.TextColor = AppStyle.TitleTextColor;

			AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Red );
			AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );
			AppButtons.FormatStandardButton((GradientButton)btnEdit, Resources.EditButton, AppStyle.ButtonColor.Grey );

            lblOrigin.Text = Resources.ConfirmOriginLablel;
            lblAptRing.Text = Resources.ConfirmAptRingCodeLabel;
            lblDestination.Text = Resources.ConfirmDestinationLabel;
            lblDateTime.Text = Resources.ConfirmDateTimeLabel;
            lblName.Text = Resources.ConfirmNameLabel;
            lblPhone.Text = Resources.ConfirmPhoneLabel;
            lblPassengers.Text = Resources.ConfirmPassengersLabel;
            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
			lblChargeType.Text = Resources.ChargeTypeLabel;

//            lblDistance.Text = Resources.RideSettingsChargeType;
            lblPrice.Text = string.Format(Resources.EstimatePrice, "");
            
            txtOrigin.Text = _parent.BookingInfo.PickupAddress.FullAddress;
            txtAptRing.Text = FormatAptRingCode(_parent.BookingInfo.PickupAddress.Apartment, _parent.BookingInfo.PickupAddress.RingCode);           
            txtDestination.Text = _parent.BookingInfo.DropOffAddress.FullAddress.HasValue() ? _parent.BookingInfo.DropOffAddress.FullAddress : Resources.ConfirmDestinationNotSpecified;            
            txtDateTime.Text = FormatDateTime(_parent.BookingInfo.PickupDate, _parent.BookingInfo.PickupDate);
            
			var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(_parent.BookingInfo.PickupAddress, _parent.BookingInfo.DropOffAddress);
            txtPrice.Text = directionInfo.Price.HasValue ? directionInfo.FormattedPrice : Resources.NotAvailable;

			SetRideSettingsFields();

            btnCancel.TouchUpInside += CancelClicked;
            btnConfirm.TouchUpInside += ConfirmClicked;
			btnEdit.TouchUpInside += EditRideSettings;
            
            View.BringSubviewToFront( bottomBar );        
        }

        void EditRideSettings (object sender, EventArgs e)
        {
            var settings = new  RideSettingsView(_parent.BookingInfo.Settings, false, false);

            settings.Closed += delegate
            {
                _parent.BookingInfo.Settings = settings.Result;
				SetRideSettingsFields();                 
            };
            
            this.NavigationController.PushViewController(settings, true);
        }

		private void SetRideSettingsFields()
		{
            var service = TinyIoCContainer.Current.Resolve<IAccountService>();            
            var companies = service.GetCompaniesList();
            var model = new RideSettingsModel(_parent.BookingInfo.Settings, companies, service.GetVehiclesList(), service.GetPaymentsList());

            txtName.Text = _parent.BookingInfo.Settings.Name;

            try
            {
                var cleaned = new string(_parent.BookingInfo.Settings.Phone.ToArray().Where(c => Char.IsDigit(c)).ToArray());
                var phone = Regex.Replace(cleaned, @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
                txtPhone.Text = phone;
            }
            catch
            {
                txtPhone.Text = _parent.BookingInfo.Settings.Phone;
            }

            txtPassengers.Text = _parent.BookingInfo.Settings.Passengers.ToString();
            txtVehiculeType.Text = model.VehicleTypeName;
			txtChargeType.Text = model.ChargeTypeName;
		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            LoadLayout();       
            
            
            if  ( AppContext.Current.WarnEstimate && _parent.BookingInfo.DropOffAddress.HasValidCoordinate() ) 
            {
                MessageHelper.Show(Resources.WarningEstimateTitle, Resources.WarningEstimate, Resources.WarningEstimateDontShow, ( ) => AppContext.Current.WarnEstimate = false); 
            }
        }

        private void LoadLayout()
        {
            this.NavigationItem.TitleView = AppContext.Current.Controller.GetTitleView(null, Resources.ConfirmationViewTitle);
        }

        private string FormatDateTime(DateTime? pickupDate, DateTime? pickupTime)
        {
            string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString() : Resources.DateToday;
            result += @" / ";
            result += pickupTime.HasValue ? pickupTime.Value.ToShortTimeString() : Resources.TimeNow;
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            
            string result = apt.HasValue() ? apt : Resources.ConfirmNoApt;
            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.ConfirmNoRingCode;
            return result;
        }

        void ConfirmClicked(object sender, EventArgs e)
        {

            var phone = txtPhone.Text;
            if (phone.Count(x => Char.IsDigit(x)) < 10)
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountInvalidPhone);
                return;
            }
            else
            {
                txtPhone.Text = new string(phone.ToArray().Where(c => Char.IsDigit(c)).ToArray());
            }

            if (Confirmed != null)
            {
                Confirmed(this, EventArgs.Empty);
            }
        }

        void CancelClicked(object sender, EventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(this, EventArgs.Empty);
            }
        }

        public CreateOrder BI
        {
            get { return _parent.BookingInfo; }
        }
        
        #endregion
    }
}

