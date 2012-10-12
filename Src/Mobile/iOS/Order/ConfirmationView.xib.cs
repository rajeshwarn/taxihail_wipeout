
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
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class ConfirmationView : UIViewController
    {
        private BookView _parent;

        public event EventHandler Confirmed;
        public event EventHandler Canceled;
//        public delegate void NoteChangedEventHandler(string note);
//
//        public event NoteChangedEventHandler NoteChanged;

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

        public ConfirmationView(BookView parent) : base("ConfirmationView", null)
        {
            _parent = parent;
            Initialize();
        }

        void Initialize()
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = true;

			lblRideSettings.TextColor = AppStyle.TitleTextColor;
			lblPickupDetails.TextColor = AppStyle.TitleTextColor;
			lblPickupDetails.Text = Resources.View_RefineAddress;

			AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Red );
			AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );
			AppButtons.FormatStandardButton((GradientButton)btnEdit, Resources.EditButton, AppStyle.ButtonColor.Grey );
			AppButtons.FormatStandardButton((GradientButton)btnEditPickupDetails, Resources.EditButton, AppStyle.ButtonColor.Grey );

            lblOrigin.Text = Resources.ConfirmOriginLablel;
            lblAptRing.Text = Resources.ConfirmAptRingCodeLabel;
            lblDestination.Text = Resources.ConfirmDestinationLabel;
            lblDateTime.Text = Resources.ConfirmDateTimeLabel;
            lblName.Text = Resources.ConfirmNameLabel;
            lblPhone.Text = Resources.ConfirmPhoneLabel;
            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
			lblChargeType.Text = Resources.ChargeTypeLabel;
			lblCompany.Text = Resources.ConfirmCompanyLabel;
			lblBuildingName.Text = Resources.HistoryDetailBuildingNameLabel;

//            lblDistance.Text = Resources.RideSettingsChargeType;
			lblPrice.Text = Resources.ApproxPrice;
            
            txtOrigin.Text = _parent.BookingInfo.PickupAddress.FullAddress;         
            txtDestination.Text = _parent.BookingInfo.DropOffAddress.FullAddress.HasValue() ? _parent.BookingInfo.DropOffAddress.FullAddress : Resources.ConfirmDestinationNotSpecified;            
            txtDateTime.Text = FormatDateTime(_parent.BookingInfo.PickupDate, _parent.BookingInfo.PickupDate);
            
			txtAptRing.Text = FormatAptRingCode(_parent.BookingInfo.PickupAddress.Apartment, _parent.BookingInfo.PickupAddress.RingCode);  
			txtBuildingName.Text = _parent.BookingInfo.PickupAddress.BuildingName;

			var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(_parent.BookingInfo.PickupAddress, _parent.BookingInfo.DropOffAddress);
            txtPrice.Text = directionInfo.Price.HasValue ? directionInfo.FormattedPrice : Resources.NotAvailable;

			SetRideSettingsFields();

            btnCancel.TouchUpInside += CancelClicked;
            btnConfirm.TouchUpInside += ConfirmClicked;
			btnEdit.TouchUpInside += EditRideSettings;
			btnEditPickupDetails.TouchUpInside += EditPickupDetails;
            
            View.BringSubviewToFront( bottomBar );    

			TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<AddressRefinedMessage>( msg => {
				_parent.BookingInfo.PickupAddress.Apartment = msg.Content.AptNumber;
				_parent.BookingInfo.PickupAddress.BuildingName = msg.Content.BuildingName;
				_parent.BookingInfo.PickupAddress.RingCode = msg.Content.RingCode;
			});
        }

        void EditPickupDetails (object sender, EventArgs e)
        {
			var args = new Dictionary<string, string>(){ {"apt", _parent.BookingInfo.PickupAddress.Apartment}, {"ringCode", _parent.BookingInfo.PickupAddress.RingCode},  {"buildingName", _parent.BookingInfo.PickupAddress.BuildingName} };
			var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
			dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(RefineAddressViewModel), args, false, MvxRequestedBy.UserAction));
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

			int nbPassenger = 0;
			int.TryParse(model.NbOfPassenger, out nbPassenger);
			var passengerFormat = nbPassenger == 1 ? Resources.NbPassenger : Resources.NbPassengers;
			txtVehiculeType.Text = model.VehicleTypeName + string.Format(passengerFormat, model.NbOfPassenger);
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
			this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);
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

