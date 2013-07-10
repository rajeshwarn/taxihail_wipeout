
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
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookEditInformation : BaseViewController<BookEditInformationViewModel>
    {
        public BookEditInformation(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BookEditInformation(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
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
            ViewModel.Load();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;           
                        

            lblName.Hidden = !ViewModel.ShowPassengerName;
            txtName.Hidden = !ViewModel.ShowPassengerName;
            
            lblPassengers.Hidden = !ViewModel.ShowPassengerNumber;
            txtNbPassengers.Hidden = !ViewModel.ShowPassengerNumber;
            
            lblPhone.Hidden = !ViewModel.ShowPassengerPhone;
            txtPhone.Hidden = !ViewModel.ShowPassengerPhone;

            contentStack.VerticalAlignement = StackPanelAlignement.Top;
            contentStack.TopOffset = 10;


            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel;
            lblChargeType.Text = Resources.ChargeTypeLabel;                     
            lblEntryCode.Text = Resources.GetValue ( "EntryCodeLabel" );
            lblApartment.Text = Resources.GetValue ( "ApartmentLabel" );
            lblName.Text = Resources.GetValue ( "PassengerNameLabel" );
            lblPassengers.Text = Resources.GetValue ( "PassengerNumberLabel" );
            lblPhone.Text = Resources.GetValue ( "PassengerPhoneLabel" );


            scrollView.ContentSize = new System.Drawing.SizeF( 320, 700 );

            txtAprtment.Ended += HandleTouchDown;
            txtEntryCode.Ended += HandleTouchDown;

            ((ModalTextField)pickerVehicleType).Configure(Resources.RideSettingsVehiculeType, ViewModel.Vehicles, ViewModel.Order.Settings.VehicleTypeId, x=> { ViewModel.SetVehicleTypeId ( x.Id );});
            ((ModalTextField)pickerChargeType).Configure(Resources.RideSettingsChargeType, ViewModel.Payments, ViewModel.Order.Settings.ChargeTypeId, x=> { ViewModel.SetChargeTypeId( x.Id ); });


            this.AddBindings(new Dictionary<object, string>() {
                { txtName, "{'Text': {'Path': 'Order.Settings.Name'}}" },
                { txtPhone, "{'Text': {'Path': 'Order.Settings.Phone'}}" },
                { txtNbPassengers, "{'Text': {'Path': 'Order.Settings.Passengers'}}" },

                { txtAprtment, "{'Text': {'Path': 'Order.PickupAddress.Apartment'}}" },
                { txtEntryCode, "{'Text': {'Path': 'Order.PickupAddress.RingCode'}}" },            
                { pickerVehicleType, "{'Text': {'Path': 'VehicleName'}}" },
                { pickerChargeType, "{'Text': {'Path': 'ChargeType'}}" },
            });

              
            this.View.ApplyAppFont ();
        }

        void HandleTouchDown (object sender, EventArgs e)
        {
            txtAprtment.ResignFirstResponder ();
            txtEntryCode.ResignFirstResponder ();
         
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);

            var btnDone = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                if( ViewModel.SaveCommand.CanExecute() )
                {
                    ViewModel.SaveCommand.Execute();
                }
            });

            this.NavigationItem.RightBarButtonItem = btnDone; 
            
        }

		
    }
}
