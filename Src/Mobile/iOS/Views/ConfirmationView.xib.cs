using System;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class ConfirmationView : BaseViewController<BookConfirmationViewModel>
    {
       

        public ConfirmationView () 
            : base(new MvxShowViewModelRequest<BookConfirmationViewModel>( null, true, new MvxRequestedBy()   ) )
        {
        }
        
        public ConfirmationView (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public ConfirmationView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

        public override void LoadView()
        {
            base.LoadView();
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            var isThriev = appSettings.ApplicationName == "Thriev";
            if (isThriev)
            {
                NSBundle.MainBundle.LoadNib ("ConfirmationView_Thriev", this, null);
            } else {
                NSBundle.MainBundle.LoadNib ("ConfirmationView", this, null);
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
            ViewModel.Load();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;

            AppButtons.FormatStandardButton((GradientButton)btnConfirm, Localize.GetValue("ConfirmButton"), AppStyle.ButtonColor.Green);          
            AppButtons.FormatStandardButton((GradientButton)btnEdit, Localize.GetValue("EditDetails"), AppStyle.ButtonColor.Grey );          
            
            

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

           

            var bindings = new [] {
                Tuple.Create<object,string>(btnConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"),
                Tuple.Create<object,string>(btnEdit   , "{'TouchUpInside':{'Path':'NavigateToEditInformations'}}"),
                Tuple.Create<object,string>(lblNameValue, "{'Text': {'Path': 'OrderName'}}"),
                Tuple.Create<object,string>(lblPhoneValue, "{'Text': {'Path': 'OrderPhone'}}"),
                Tuple.Create<object,string>(lblPassengersValue, "{'Text': {'Path': 'OrderPassengerNumber'}}"),
                Tuple.Create<object,string>(lblApartmentValue, "{'Text': {'Path': 'OrderApt'}}"),
                Tuple.Create<object,string>(lblEntryCodeValue, "{'Text': {'Path': 'OrderRingCode'}}"),
                Tuple.Create<object,string>(lblVehicleTypeValue, "{'Text': {'Path': 'RideSettings.VehicleTypeName'}}"),
                Tuple.Create<object,string>(lblChargeTypeValue, "{'Text': {'Path': 'RideSettings.ChargeTypeName'}}"),
                Tuple.Create<object,string>(lblLargeBagsValue, "{'Text': {'Path': 'OrderLargeBagsNumber'}}"),
                Tuple.Create<object,string>(lblPickupValue, "{'Text': {'Path': 'Order.PickupAddress.DisplayAddress'}}"),
                Tuple.Create<object,string>(lblDestinationValue, "{'Text': {'Path': 'Order.DropOffAddress.DisplayAddress'}}"),
                Tuple.Create<object,string>(lblFareValue, "{'Text': {'Path': 'FareEstimate'}}"),
            }
                .Where(x=> x.Item1 != null )
                .ToDictionary(x=>x.Item1, x=>x.Item2);
        
            this.AddBindings(bindings);
            
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
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_BookingDetail"), true);
          
        }
    }
}

