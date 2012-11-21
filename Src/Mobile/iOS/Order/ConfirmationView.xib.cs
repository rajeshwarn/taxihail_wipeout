
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
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class ConfirmationView : MvxBindingTouchViewController<BookConfirmationViewModel>
    {
        #region Constructors
        
        public ConfirmationView() 
            : base(new MvxShowViewModelRequest<BookingStatusViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public ConfirmationView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public ConfirmationView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

        public 
        void Initialize()
        {
        }

        public CreateOrder Order
        {
            get { return ViewModel.Order; }
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
            


			

			SetRideSettingsFields();


			btnEdit.TouchUpInside += EditRideSettings;
			btnEditPickupDetails.TouchUpInside += EditPickupDetails;
            
            View.BringSubviewToFront( bottomBar );    


            ViewModel.OnViewLoaded();

            this.AddBindings(new Dictionary<object, string>() {
                { btnCancel, "{'TouchUpInside':{'Path':'CancelOrderCommand'}}"},                
                { btnConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},
                { txtOrigin, "{'Text': {'Path': 'Order.PickupAddress.FullAddress'}}" },
                { txtDestination, "{'Text': {'Path': 'Order.DropOffAddress.FullAddress', 'Converter': 'EmptyToResourceConverter', 'ConverterParameter': 'ConfirmDestinationNotSpecified'}}" },
                { txtDateTime, "{'Text': {'Path': 'FormattedPickupDate'}}" },
                { txtAptRing, "{'Text': {'Path': 'AptRingCode'}}" },
                { txtBuildingName, "{'Text': {'Path': 'BuildingName'}}" },
                { txtPrice, "{'Text': {'Path': 'FareEstimate'}}" },
                { txtName, "{'Text': {'Path': 'Order.Settings.Name'}}" },
               
            });
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
            this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);

        }
		  
        #endregion
    }
}

