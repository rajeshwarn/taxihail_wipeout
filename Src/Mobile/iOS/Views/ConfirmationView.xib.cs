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
using apcurium.MK.Booking.Mobile.Client;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class ConfirmationView : BaseViewController<BookConfirmationViewModel>
    {
       

        public ConfirmationView () 
            : base(new MvxShowViewModelRequest<BookConfirmationViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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
            
            AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );          
            AppButtons.FormatStandardButton((GradientButton)btnEdit, Resources.GetValue ( "EditDetails" ), AppStyle.ButtonColor.Grey );          
            
            
            
            lblName.Hidden = !ViewModel.ShowPassengerName;
            lblNameValue.Hidden = !ViewModel.ShowPassengerName;
            
            lblPassengers.Hidden = !ViewModel.ShowPassengerNumber;
            lblPassengersValue.Hidden = !ViewModel.ShowPassengerNumber;
            
            lblPhone.Hidden = !ViewModel.ShowPassengerPhone;
            lblPhoneValue.Hidden = !ViewModel.ShowPassengerPhone;
            
            
            var countHidden = Params.Get ( ViewModel.ShowPassengerName , ViewModel.ShowPassengerNumber, ViewModel.ShowPassengerPhone ).Count ( s => !s );
            
            topStack.Offset = topStack.Offset + countHidden ;
            
            
            
            lblVehiculeType.Text = Resources.ConfirmVehiculeTypeLabel + ": "; 
            lblChargeType.Text = Resources.ChargeTypeLabel + ": ";                      
            lblEntryCode.Text = Resources.GetValue ( "EntryCodeLabel" )+ ": ";
            lblApartment.Text = Resources.GetValue ( "ApartmentLabel" )+ ": ";
            lblNoteDriver.Text = Resources.GetValue ( "NotesToDriveLabel" )+ ": ";
            
            lblName.Text = Resources.GetValue ( "PassengerNameLabel" )+ ": ";
            lblPassengers.Text = Resources.GetValue ( "PassengerNumberLabel" )+ ": ";
            lblPhone.Text = Resources.GetValue ( "PassengerPhoneLabel" )+ ": ";
            
            scrollView.ContentSize = new System.Drawing.SizeF( 320, 700 );
            
            txtNotes.Ended += HandleTouchDown;
            txtNotes.Started += NoteStartedEdit;
            txtNotes.Changed += (sender, e) => ViewModel.Order.Note = txtNotes.Text;
            
            lblNameValue.TextColor = AppStyle.DarkText;
            lblNameValue.Font = AppStyle.GetBoldFont (lblNameValue.Font.PointSize);
            
            lblPhoneValue.TextColor = AppStyle.DarkText;
            lblPhoneValue.Font = AppStyle.GetBoldFont (lblPhoneValue.Font.PointSize);
            
            lblPassengersValue.TextColor = AppStyle.DarkText;
            lblPassengersValue.Font = AppStyle.GetBoldFont (lblPassengersValue.Font.PointSize);
            
            lblApartmentValue.TextColor = AppStyle.DarkText;
            lblApartmentValue.Font = AppStyle.GetBoldFont (lblApartmentValue.Font.PointSize);
            
            lblEntryCodeValue.TextColor = AppStyle.DarkText;
            lblEntryCodeValue.Font = AppStyle.GetBoldFont (lblEntryCodeValue.Font.PointSize);
            
            lblVehicleTypeValue.TextColor = AppStyle.DarkText;
            lblVehicleTypeValue.Font = AppStyle.GetBoldFont (lblVehicleTypeValue.Font.PointSize);
            
            lblChargeTypeValue.TextColor = AppStyle.DarkText;
            lblChargeTypeValue.Font = AppStyle.GetBoldFont (lblChargeTypeValue.Font.PointSize);
            
            
            this.AddBindings(new Dictionary<object, string>() {
                { btnConfirm, "{'TouchUpInside':{'Path':'ConfirmOrderCommand'}}"},
                { btnEdit, "{'TouchUpInside':{'Path':'NavigateToEditInformations'}}"},
                { lblNameValue, "{'Text': {'Path': 'OrderName'}}" },
                { lblPhoneValue, "{'Text': {'Path': 'OrderPhone'}}" },
                { lblPassengersValue, "{'Text': {'Path': 'OrderPassengerNumber'}}" },
                
                { lblApartmentValue, "{'Text': {'Path': 'OrderApt'}}" },
                { lblEntryCodeValue, "{'Text': {'Path': 'OrderRingCode'}}" },
                { lblVehicleTypeValue, "{'Text': {'Path': 'VehicleName'}}" },
                { lblChargeTypeValue, "{'Text': {'Path': 'ChargeType'}}" },
            });
            
            
            
            this.View.ApplyAppFont ();
        }

        private void OffsetControls (float offset, params UIView[] controls)
        {
            foreach (var item in controls) {
                item.Frame = new RectangleF (item.Frame.X, item.Frame.Y + offset, item.Frame.Width, item.Frame.Height);
            }
        }

        void NoteStartedEdit (object sender, EventArgs e)
        {
            scrollView.SetContentOffset (new System.Drawing.PointF (0, 208.5f), true);

        }

        void HandleTouchDown (object sender, EventArgs e)
        {

            txtNotes.ResignFirstResponder ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            this.NavigationItem.TitleView = new TitleView (null, Resources.View_BookingDetail, true);
          
        }
    }
}

