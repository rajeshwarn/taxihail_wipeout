
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

            			
			lblPickupDetails.TextColor = AppStyle.TitleTextColor;
			lblPickupDetails.Text = Resources.View_RefineAddress;

			AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.CancelBoutton, AppStyle.ButtonColor.Red );
			AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );			

            lblOrigin.Text = Resources.ConfirmOriginLablel;
            lblAptRing.Text = Resources.ConfirmAptRingCodeLabel;
            lblDestination.Text = Resources.ConfirmDestinationLabel;
            lblDateTime.Text = Resources.ConfirmDateTimeLabel;
            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
			lblChargeType.Text = Resources.ChargeTypeLabel;			
			

			lblPrice.Text = Resources.ApproxPrice;


            ((ModalTextField)pickerVehicleType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.Order.Settings.VehicleTypeId, x=> {
                ViewModel.SetVehicleTypeId ( x.Id );});

            ((ModalTextField)pickerChargeType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Payments, ViewModel.Order.Settings.ChargeTypeId , x=> {
                ViewModel.SetChargeTypeId( x.Id ); });

			View.BringSubviewToFront( bottomBar );    

            this.AddBindings(new Dictionary<object, string>() {
                { btnCancel, "{'TouchUpInside':{'Path':'CancelOrderCommand'}}"},                
                { btnConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},
                { txtOrigin, "{'Text': {'Path': 'Order.PickupAddress.FullAddress'}}" },

                { txtDestination, "{'Text': {'Path': 'Order.DropOffAddress.FullAddress', 'Converter': 'EmptyToResource', 'ConverterParameter': 'ConfirmDestinationNotSpecified'}}" },
                { txtDateTime, "{'Text': {'Path': 'FormattedPickupDate'}}" },
                { txtPrice, "{'Text': {'Path': 'FareEstimate'}}" },
                { pickerVehicleType, "{'Text': {'Path': 'VehicleName'}}" },
                { pickerChargeType, "{'Text': {'Path': 'ChargeType'}}" },
            });



            ViewModel.OnViewLoaded();
            this.View.ApplyAppFont ();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);
        }
    }
}

