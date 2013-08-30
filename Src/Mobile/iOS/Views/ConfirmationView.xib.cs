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
using apcurium.MK.Booking.Mobile.Infrastructure;

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

        public override void LoadView()
        {
            base.LoadView();
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            bool isThriev = appSettings.ApplicationName == "Thriev";
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
            
            AppButtons.FormatStandardButton((GradientButton)btnConfirm, Resources.ConfirmButton, AppStyle.ButtonColor.Green );          
            AppButtons.FormatStandardButton((GradientButton)btnEdit, Resources.GetValue ( "EditDetails" ), AppStyle.ButtonColor.Grey );          
            
            

            lblName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            lblNameValue.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            
            lblPassengers.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerNumber);
            lblPassengersValue.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerNumber);
            
            lblPhone.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerPhone);
            lblPhoneValue.Maybe(x=>x.Hidden = !ViewModel.ShowPassengerPhone);
            
            
            var countHidden = Params.Get ( ViewModel.ShowPassengerName , ViewModel.ShowPassengerNumber, ViewModel.ShowPassengerPhone ).Count ( s => !s );
            
            topStack.Offset = topStack.Offset + countHidden ;
            
            
            
            lblVehiculeType.Maybe(x=>x.Text = Resources.ConfirmVehiculeTypeLabel + ": "); 
            lblChargeType.Maybe(x=>x.Text = Resources.ChargeTypeLabel + ": ");                      
            lblEntryCode.Maybe(x=>x.Text = Resources.GetValue ( "EntryCodeLabel" )+ ": ");
            lblApartment.Maybe(x=>x.Text = Resources.GetValue ( "ApartmentLabel" )+ ": ");
            lblNoteDriver.Maybe(x=>x.Text = Resources.GetValue ( "NotesToDriveLabel" )+ ": ");
            
            lblName.Maybe(x=>x.Text = Resources.GetValue ( "PassengerNameLabel" )+ ": ");
            lblPassengers.Maybe(x=>x.Text = Resources.GetValue ( "PassengerNumberLabel" )+ ": ");
            lblPhone.Maybe(x=>x.Text = Resources.GetValue ( "PassengerPhoneLabel" )+ ": ");
            
            scrollView.ContentSize = new System.Drawing.SizeF( 320, 700 );
            
            txtNotes.Ended += HandleTouchDown;
            txtNotes.Started += NoteStartedEdit;
            txtNotes.Changed += (sender, e) => ViewModel.Order.Note = txtNotes.Text;

            // Apply Font style to values
            new [] { lblNameValue, lblPhoneValue, lblPassengersValue, lblApartmentValue, lblEntryCodeValue, lblVehicleTypeValue, lblChargeTypeValue }
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
                Tuple.Create<object,string>(lblVehicleTypeValue, "{'Text': {'Path': 'VehicleName'}}"),
                Tuple.Create<object,string>(lblChargeTypeValue, "{'Text': {'Path': 'ChargeType'}}"),
            }
                .Where(x=> x.Item1 != null )
                .ToDictionary(x=>x.Item1, x=>x.Item2);

            this.AddBindings(bindings);
            
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

