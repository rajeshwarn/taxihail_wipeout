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
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class ConfirmationView : MvxBindingTouchViewController<BookConfirmationViewModel>
    {
       

        public ConfirmationView() 
            : base(new MvxShowViewModelRequest<BookConfirmationViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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
            ViewModel.OnViewLoaded();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;
                        					
			AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );			


            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
			lblChargeType.Text = Resources.ChargeTypeLabel;						
            lblEntryCode.Text = Resources.GetValue ( "EntryCodeLabel" );
            lblApartment.Text = Resources.GetValue ( "ApartmentLabel" );
            lblNoteDriver.Text = Resources.GetValue ( "NotesToDriveLabel" );

            txtNotes.Ended += HandleTouchDown;
            txtApartment.Ended += HandleTouchDown;
            txtEntryCode.Ended += HandleTouchDown;
            txtNotes.Started += NoteStartedEdit;

            scrollView.ContentSize = new System.Drawing.SizeF( 320, 700 );


            ((ModalTextField)pickerVehicleType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.Order.Settings.VehicleTypeId, x=> {
                ViewModel.SetVehicleTypeId ( x.Id );});

            ((ModalTextField)pickerChargeType).Configure(Resources.RideSettingsChargeType, ViewModel.Payments, ViewModel.Order.Settings.ChargeTypeId , x=> {
                ViewModel.SetChargeTypeId( x.Id ); });

            this.AddBindings(new Dictionary<object, string>() {
                { btnConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},
                { txtApartment, "{'Text': {'Path': 'Order.PickupAddress.Apartment'}}" },
                { txtEntryCode, "{'Text': {'Path': 'Order.PickupAddress.RingCode'}}" },
                { txtNotes, "{'Text': {'Path': 'Order.PickupAddress.RingCode'}}" },
                { pickerVehicleType, "{'Text': {'Path': 'VehicleName'}}" },
                { pickerChargeType, "{'Text': {'Path': 'ChargeType'}}" },
            });

            pickerVehicleType.Initialize();
            pickerChargeType.Initialize();
            this.View.ApplyAppFont ();
        }


        void NoteStartedEdit (object sender, EventArgs e)
        {
            scrollView.SetContentOffset( new System.Drawing.PointF( 0, 208.5f ), true );

        }
        void HandleTouchDown (object sender, EventArgs e)
        {
            txtApartment.ResignFirstResponder ();
            txtEntryCode.ResignFirstResponder ();
            txtNotes.ResignFirstResponder ();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);
          
        }
    }
}

