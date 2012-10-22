
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

        public ConfirmationView(CreateOrder order) : base("ConfirmationView", null)
        {
            Order = order;
            Initialize();
        }

        void Initialize()
        {
        }

        public CreateOrder Order
        {
            get;
            set;
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

			lblPrice.Text = Resources.ApproxPrice;
            
            txtOrigin.Text = Order.PickupAddress.FullAddress;         
            txtDestination.Text = Order.DropOffAddress.FullAddress.HasValue() ? Order.DropOffAddress.FullAddress : Resources.ConfirmDestinationNotSpecified;            
            txtDateTime.Text = FormatDateTime(Order.PickupDate);




			txtAptRing.Text = FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);  
            txtBuildingName.Text = FormatBuildingName( Order.PickupAddress.BuildingName );

			var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(Order.PickupAddress, Order.DropOffAddress);
            txtPrice.Text = directionInfo.Price.HasValue ? directionInfo.FormattedPrice : Resources.NotAvailable;

			SetRideSettingsFields();

            btnCancel.TouchUpInside += CancelClicked;
            btnConfirm.TouchUpInside += ConfirmClicked;
			btnEdit.TouchUpInside += EditRideSettings;
			btnEditPickupDetails.TouchUpInside += EditPickupDetails;
            
            View.BringSubviewToFront( bottomBar );    

			TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<AddressRefinedMessage>( msg => {
				Order.PickupAddress.Apartment = msg.Content.AptNumber;
				Order.PickupAddress.BuildingName = msg.Content.BuildingName;
				Order.PickupAddress.RingCode = msg.Content.RingCode;

                txtAptRing.Text = FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);  
                txtBuildingName.Text = FormatBuildingName(Order.PickupAddress.BuildingName);


			});
        }

        private string FormatBuildingName( string buildingName )
        {
            if ( buildingName.HasValue() )
            {
                return buildingName;
            }
            else
            {
                return Resources.GetValue( "HistoryDetailBuildingNameNotSpecified" );
            }
        }
        void EditPickupDetails (object sender, EventArgs e)
        {
			var args = new Dictionary<string, string>(){ {"apt", Order.PickupAddress.Apartment}, {"ringCode", Order.PickupAddress.RingCode},  {"buildingName", Order.PickupAddress.BuildingName} };
			var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
			dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(RefineAddressViewModel), args, false, MvxRequestedBy.UserAction));
        }

        void EditRideSettings (object sender, EventArgs e)
        {
            var settings = new  RideSettingsView(Order.Settings, false, false);

            settings.Closed += delegate
            {
                Order.Settings = settings.Result;
				SetRideSettingsFields();                 
            };
            
            this.NavigationController.PushViewController(settings, true);
        }

		private void SetRideSettingsFields()
		{
            var service = TinyIoCContainer.Current.Resolve<IAccountService>();            
            var companies = service.GetCompaniesList();
            var model = new RideSettingsModel(Order.Settings, companies, service.GetVehiclesList(), service.GetPaymentsList());

            txtName.Text = Order.Settings.Name;

            try
            {
                var cleaned = new string(Order.Settings.Phone.ToArray().Where(c => Char.IsDigit(c)).ToArray());
                var phone = Regex.Replace(cleaned, @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
                txtPhone.Text = phone;
            }
            catch
            {
                txtPhone.Text = Order.Settings.Phone;
            }

			int nbPassenger = 0;
			int.TryParse(model.NbOfPassenger, out nbPassenger);
			var passengerFormat = nbPassenger == 1 ? Resources.NbPassenger : Resources.NbPassengers;
			txtVehiculeType.Text = model.VehicleTypeName + string.Format(passengerFormat, model.NbOfPassenger);
			txtChargeType.Text = model.ChargeTypeName;

            var company = model.CompanyList.FirstOrDefault( c=>c.Id == Order.Settings.ProviderId );
            if ( company == null )
            {
                company = model.CompanyList.First( c=>c.IsDefault.HasValue && c.IsDefault.Value );
                Order.Settings.ProviderId = company.Id;
            }

            txtCompany.Text = company.Display;

		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            LoadLayout();       
            
            
            if  ( AppContext.Current.WarnEstimate && Order.DropOffAddress.HasValidCoordinate() ) 
            {
                MessageHelper.Show(Resources.WarningEstimateTitle, Resources.WarningEstimate, Resources.WarningEstimateDontShow, ( ) => AppContext.Current.WarnEstimate = false); 
            }
        }

        private void LoadLayout()
        {
			this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);
        }

//        private string FormatDateTime(DateTime? pickupDate, DateTime? pickupTime)
//        {
//            string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString() : Resources.DateToday;
//            result += @" / ";
//            result += pickupTime.HasValue ? pickupTime.Value.ToShortTimeString() : Resources.TimeNow;
//            return result;
//        }

        private string FormatDateTime(DateTime? pickupDate )
        {
            string format = "{0:ddd, MMM d}, {0:h:mm tt}";
            string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.TimeNow;
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
                Confirmed(Order, EventArgs.Empty);
            }
        }

        void CancelClicked(object sender, EventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(this, EventArgs.Empty);
            }
        }

//        public CreateOrder BI
//        {
//            get { return Order; }
//        }
//        
        #endregion
    }
}

