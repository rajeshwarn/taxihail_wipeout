using System;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MK.Common.iOS.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookEditInformation : BaseViewController<BookEditInformationViewModel>
    {
        public BookEditInformation() 
			: base("BookEditInformation", null)
        {
        }

        public override void LoadView()
        {
            base.LoadView();
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            var isThriev = appSettings.Data.ApplicationName == "Thriev";
            if (isThriev)
            {
                NSBundle.MainBundle.LoadNib ("BookEditInformation_Thriev", this, null);
            } else {
                NSBundle.MainBundle.LoadNib ("BookEditInformation", this, null);
            }
        }
        
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
            ChangeRightBarButtonFontToBold();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.OnViewLoaded();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;           

            lblName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            txtName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            
            lblPassengers.Maybe( x=> x.Hidden = !ViewModel.ShowPassengerNumber);
            txtNbPassengers.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerNumber);
            
            lblPhone.Maybe(x => x.Hidden = !ViewModel.ShowPassengerPhone);
            txtPhone.Maybe(x => x.Hidden = !ViewModel.ShowPassengerPhone);

            contentStack.Maybe(x => x.VerticalAlignement = StackPanelAlignement.Top);
            contentStack.Maybe(x => x.TopOffset = 10);

            lblVehiculeType.Maybe(x => x.Text = Localize.GetValue("ConfirmVehiculeTypeLabel"));
            lblChargeType.Maybe(x => x.Text = Localize.GetValue("ChargeTypeLabel"));                     
            lblEntryCode.Maybe(x => x.Text = Localize.GetValue("EntryCodeLabel"));
            lblApartment.Maybe(x => x.Text = Localize.GetValue ("ApartmentLabel"));
            lblName.Maybe(x => x.Text = Localize.GetValue ("PassengerNameLabel" ));
            lblPassengers.Maybe(x => x.Text = Localize.GetValue ("PassengerNumberLabel"));
            lblLargeBags.Maybe(x => x.Text = Localize.GetValue ("LargeBagsLabel"));
            lblPhone.Maybe(x => x.Text = Localize.GetValue ("PassengerPhoneLabel"));

            scrollView.ContentSize = new SizeF( 320, 700 );

            txtAprtment.Maybe(x => x.Ended += HandleTouchDown);
            txtEntryCode.Maybe(x => x.Ended += HandleTouchDown);

            pickerVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), 
                                        ()=> ViewModel.Vehicles, 
                                        ViewModel.VehicleTypeId, 
                                        x => ViewModel.SetVehicleTypeId(x.Id), 
                                        ViewModel.OnPropertyChanged().Where( property => property == "Vehicles") );

            pickerChargeType.Configure(Localize.GetValue("RideSettingsChargeType"),
                                       ()=>ViewModel.Payments, 
                                       ViewModel.ChargeTypeId, 
                                       x => ViewModel.SetChargeTypeId(x.Id), 
                                       ViewModel.OnPropertyChanged().Where( property => property == "Payments"));

			var set = this.CreateBindingSet<BookEditInformation, BookEditInformationViewModel>();

			set.BindSafe(txtName)
				.For(v => v.Text)
				.To(vm => vm.Order.Settings.Name);

			set.BindSafe(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Order.Settings.Phone);

			set.BindSafe(txtNbPassengers)
				.For(v => v.Text)
				.To(vm => vm.Order.Settings.Passengers);

			set.BindSafe(txtLargeBags)
				.For(v => v.Text)
				.To(vm => vm.Order.Settings.LargeBags);

			set.BindSafe(txtAprtment)
				.For(v => v.Text)
				.To(vm => vm.Order.PickupAddress.Apartment);

			set.BindSafe(txtEntryCode)
				.For(v => v.Text)
				.To(vm => vm.Order.PickupAddress.RingCode);

			set.BindSafe(pickerVehicleType)
				.For("Text")
				.To(vm => vm.VehicleName);

			set.BindSafe(pickerChargeType)
				.For("Text")
				.To(vm => vm.ChargeType);

			set.Apply ();
              
            View.ApplyAppFont ();
        }

        void HandleTouchDown (object sender, EventArgs e)
        {
            txtAprtment.Maybe(x => x.ResignFirstResponder ());
            txtEntryCode.Maybe(x => x.ResignFirstResponder ());
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
			NavigationItem.Title = Localize.GetValue("View_BookingDetail");

            var btnDone = new UIBarButtonItem(Localize.GetValue("Done"), UIBarButtonItemStyle.Plain, delegate
            {
                if( ViewModel.SaveCommand.CanExecute() )
                {
                    ViewModel.SaveCommand.Execute();
                }
            });

            NavigationItem.RightBarButtonItem = btnDone; 
        }
    }
}
