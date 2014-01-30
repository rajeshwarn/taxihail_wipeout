using System;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MK.Common.iOS.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ConfirmationView : BaseViewController<BookConfirmationViewModel>
    {
        public ConfirmationView () 
			: base("ConfirmationView", null)
        {
        }

		public override string NibName
		{
			get
			{
				var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
                var isThriev = appSettings.Data.ApplicationName == "Thriev";
				return isThriev
					? "ConfirmationView_Thriev"
					: "ConfirmationView";
			}
		}

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;


        }

        public override void ViewDidLoad ()
        {
		
            base.ViewDidLoad();
            ViewModel.OnViewLoaded();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;

            AppButtons.FormatStandardButton((GradientButton)btnConfirm, Localize.GetValue("Confirm"), AppStyle.ButtonColor.Green);          
            AppButtons.FormatStandardButton((GradientButton)btnEdit, Localize.GetValue("Edit"), AppStyle.ButtonColor.Grey );          
            
            

            lblName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            lblNameValue.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            
            lblPassengers.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerNumber);
            lblPassengersValue.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerNumber);
            
            lblPhone.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerPhone);
            lblPhoneValue.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerPhone);
            
            topStack.Maybe(x => {
                var countHidden = Params.Get ( ViewModel.ShowPassengerName , ViewModel.ShowPassengerNumber, ViewModel.ShowPassengerPhone ).Count ( s => !s );
                x.Offset = x.Offset + countHidden ;
            });


            lblVehiculeType.Maybe(x => x.Text = Localize.GetValue("ConfirmVehiculeTypeLabel") + ":");
            lblChargeType.Maybe(x => x.Text = Localize.GetValue("ChargeTypeLabel") + ":");                      
            lblEntryCode.Maybe(x=>x.Text = Localize.GetValue ( "EntryCodeLabel" )+ ":");
            lblApartment.Maybe(x=>x.Text = Localize.GetValue ( "ApartmentLabel" )+ ":");
            lblNoteDriver.Maybe(x=>x.Text = Localize.GetValue ( "NotesToDriveLabel" )+ ":");
            
            lblName.Maybe(x=>x.Text = Localize.GetValue ( "PassengerNameLabel" )+ ":");
            lblPassengers.Maybe(x=>x.Text = Localize.GetValue ( "PassengerNumberLabel" )+ ":");
            lblLargeBags.Maybe(x=>x.Text = Localize.GetValue ( "LargeBagsLabel" )+ ":");
            lblPhone.Maybe(x=>x.Text = Localize.GetValue ( "PassengerPhoneLabel" )+ ":");

            lblPickup.Maybe(x => x.Text = Localize.GetValue("ConfirmOriginLablel") + ":");
            lblDestination.Maybe(x => x.Text = Localize.GetValue("ConfirmDestinationLabel") + ":");
            lblFare.Maybe(x => x.Text = Localize.GetValue("EstimatePrice") + ":");

            txtNotes.Placeholder = Localize.GetValue("NotesToDriverHint");
            
            scrollView.ContentSize = new SizeF( 320, 700 );
            
            txtNotes.Ended += HandleTouchDown;
            txtNotes.Started += NoteStartedEdit;
            txtNotes.Changed += (sender, e) => ViewModel.Order.Note = txtNotes.Text;

            // Apply Font style to values
            new [] { 
                lblNameValue,
                lblPhoneValue,
                lblPassengersValue,
                lblLargeBagsValue,
                lblApartmentValue,
                lblEntryCodeValue,
                lblVehicleTypeValue,
                lblChargeTypeValue,
                lblPickupValue,
                lblDestinationValue,
                lblFareValue
            }
                .Where(x => x != null)
                .ForEach(x => x.TextColor = AppStyle.DarkText)
                .ForEach(x => x.Font = AppStyle.GetBoldFont(x.Font.PointSize));

			var set = this.CreateBindingSet<ConfirmationView, BookConfirmationViewModel>();

			set.BindSafe(btnConfirm)
				.For("TouchUpInside")
				.To(vm => vm.ConfirmOrderCommand);

			set.BindSafe(btnEdit)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToEditInformations);

			set.BindSafe(lblNameValue)
				.For("Text")
				.To(vm => vm.OrderName);

			set.BindSafe(lblPhoneValue)
				.For("Text")
				.To(vm => vm.OrderPhone);

			set.BindSafe(lblPassengersValue)
				.For("Text")
				.To(vm => vm.OrderPassengerNumber);

			set.BindSafe(lblApartmentValue)
				.For("Text")
				.To(vm => vm.OrderApt);

			set.BindSafe(lblEntryCodeValue)
				.For("Text")
				.To(vm => vm.OrderRingCode);

			set.BindSafe(lblVehicleTypeValue)
				.For("Text")
				.To(vm => vm.RideSettings.VehicleTypeName);

			set.BindSafe(lblChargeTypeValue)
				.For("Text")
				.To(vm => vm.RideSettings.ChargeTypeName);

			set.BindSafe(lblLargeBagsValue)
				.For("Text")
				.To(vm => vm.OrderLargeBagsNumber);

			set.BindSafe(lblPickupValue)
				.For("Text")
				.To(vm => vm.Order.PickupAddress.DisplayAddress);

			set.BindSafe(lblDestinationValue)
				.For("Text")
				.To(vm => vm.Order.DropOffAddress.DisplayAddress);

			set.BindSafe(lblFareValue)
				.For("Text")
				.To(vm => vm.FareEstimate);

			set.Apply ();

            View.ApplyAppFont ();
        }

        private void OffsetControls (float offset, params UIView[] controls)
        {
            foreach (var item in controls) {
                item.Frame = new RectangleF (item.Frame.X, item.Frame.Y + offset, item.Frame.Width, item.Frame.Height);
            }
        }

        void NoteStartedEdit (object sender, EventArgs e)
        {
            scrollView.SetContentOffset (new PointF (0, 208.5f), true);

        }

        void HandleTouchDown (object sender, EventArgs e)
        {

            txtNotes.ResignFirstResponder ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
			NavigationItem.Title = Localize.GetValue("View_BookingDetail");
          
        }
    }
}

