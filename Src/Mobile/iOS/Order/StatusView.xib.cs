using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.MapUtilities;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class StatusView : MvxBindingTouchViewController<BookingStatusViewModel>
    {
        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public StatusView () 
            : base(new MvxShowViewModelRequest<BookingStatusViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public StatusView (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public StatusView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
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
            base.ViewDidLoad ();

            try {
                NavigationItem.HidesBackButton = true;                
                View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

                View.BringSubviewToFront (lblTitle);
                lblTitle.Text = Resources.LoadingMessage;

                btnChangeBooking.SetTitle (Resources.ChangeBookingSettingsButton, UIControlState.Normal);
             
                AppButtons.FormatStandardButton ((GradientButton)btnCall, Resources.StatusCallButton, AppStyle.ButtonColor.Black);
                AppButtons.FormatStandardButton ((GradientButton)btnCancel, Resources.StatusCancelButton, AppStyle.ButtonColor.Red);
                AppButtons.FormatStandardButton ((GradientButton)btnNewRide, Resources.StatusNewRideButton, AppStyle.ButtonColor.Green);
                this.NavigationItem.TitleView = new TitleView (null, Resources.GenericTitle, false);
                
                View.BringSubviewToFront (bottomBar);

                this.AddBindings (new Dictionary<object, string> ()                            
                {
                    { mapStatus, "{'Pickup':{'Path':'Pickup.Model'}, 'TaxiLocation':{'Path':'OrderStatusDetail'}, 'MapCenter':{'Path':'MapCenter'} }" },
                    { lblTitle, "{'Text':{'Path':'StatusInfoText'}}" },
                    { lblStatus, "{'Text':{'Path':'ConfirmationNoTxt'}}" },
                    { btnCancel, "{'TouchUpInside': {'Path': 'CancelOrder'}}" },
                    { btnCall, "{'Hidden':{'Path':'IsCallButtonVisible', 'Converter':'BoolInverter'}, 'Enabled':{'Path':'IsCallButtonVisible'}, 'TouchUpInside':{'Path':'CallCompany'}}" },
                    { btnNewRide, "{'TouchUpInside': {'Path': 'NewRide'}}" }
                });
                mapStatus.Delegate = new AddressMapDelegate ();
                mapStatus.PickupIsActive = false;
                mapStatus.DropoffIsActive = false;
            
            } catch (Exception ex) {
                Logger.LogError (ex);
            }

            this.View.ApplyAppFont ();
        }

        #endregion
    }

}

