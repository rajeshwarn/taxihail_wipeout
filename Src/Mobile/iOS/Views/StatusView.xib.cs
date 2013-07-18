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
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class StatusView : BaseViewController<BookingStatusViewModel>
    {
        #region Constructors

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

        #endregion

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = false;  

            if ( NavigationController.ViewControllers.Any ( vc=>vc is ConfirmationView ) )
            {
                var newNavStack = NavigationController.ViewControllers.Where (  vc=>!(vc is ConfirmationView ));
                NavigationController.SetViewControllers ( newNavStack.ToArray () , false );
            }
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            try {
                ViewModel.Load();

                NavigationItem.HidesBackButton = true;                
                View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

                View.BringSubviewToFront (statusBar);

                statusBar.Initialize ( topVisibleStatus, topSlidingStatus );
                lblConfirmation.Text = Resources.LoadingMessage;
                txtDriver.Text = Resources.DriverInfoDriver;
                txtDriver.TextColor = AppStyle.GreyText;
                txtLicence.Text = Resources.DriverInfoLicence;
                txtLicence.TextColor = AppStyle.GreyText;
                txtTaxiType.Text = Resources.DriverInfoTaxiType;
                txtTaxiType.TextColor = AppStyle.GreyText;
				txtMake.Text = Resources.DriverInfoMake;
				txtMake.TextColor = AppStyle.GreyText;
				txtModel.Text = Resources.DriverInfoModel;
				txtModel.TextColor = AppStyle.GreyText;
                txtColor.Text = Resources.DriverInfoColor;
                txtColor.TextColor = AppStyle.GreyText;

                btnChangeBooking.SetTitle (Resources.ChangeBookingSettingsButton, UIControlState.Normal);

                topSlidingStatus.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
                topVisibleStatus.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/backPickupDestination.png"));

                viewLine.Frame = new System.Drawing.RectangleF( 0,topSlidingStatus.Bounds.Height -1, topSlidingStatus.Bounds.Width, 1 );

                AppButtons.FormatStandardButton ((GradientButton)btnCallDriver, "", AppStyle.ButtonColor.Grey, "Assets/phone.png");

                AppButtons.FormatStandardButton ((GradientButton)btnCall, Resources.StatusCallButton, AppStyle.ButtonColor.Black);
                AppButtons.FormatStandardButton ((GradientButton)btnCancel, Resources.StatusCancelButton, AppStyle.ButtonColor.Red);
                AppButtons.FormatStandardButton ((GradientButton)btnNewRide, Resources.StatusNewRideButton, AppStyle.ButtonColor.Green);
                AppButtons.FormatStandardButton ((GradientButton)btnPay, Resources.StatusPayButton, AppStyle.ButtonColor.Green);

                this.NavigationItem.TitleView = new TitleView (null, Resources.GenericTitle, false);
                
                View.BringSubviewToFront (bottomBar);

				ViewModel.PropertyChanged+= (sender, e) => {
					InvokeOnMainThread(()=>
					{
						UpdateTopSlidingStatus(e.PropertyName);
					});
				};

                if ( ViewModel.IsCallButtonVisible )
                {
                    btnCancel.SetFrame(8, btnCancel.Frame.Y,  btnCancel.Frame.Width,  btnCancel.Frame.Height );
                    btnCall.SetFrame( 320 - 8 - btnCall.Frame.Width ,  btnCall.Frame.Y,  btnCall.Frame.Width,  btnCall.Frame.Height );
                    btnPay.SetFrame(btnCancel.Frame);

                    var callFrame = btnCall.Frame;
                    UpdateCallButtonSize (callFrame);
                    ViewModel.PropertyChanged+= (sender, e) => {
                        InvokeOnMainThread(()=>
                        {
                            UpdateCallButtonSize (callFrame);
                        });
                    };
                }

                lblDriver.TextColor = AppStyle.DarkText;
                lblLicence.TextColor = AppStyle.DarkText;
                lblTaxiType.TextColor = AppStyle.DarkText;
                lblMake.TextColor = AppStyle.DarkText;
                lblModel.TextColor = AppStyle.DarkText;
                lblColor.TextColor = AppStyle.DarkText;

                lblConfirmation.TextColor = AppStyle.GreyText;
                lblStatus.TextColor = AppStyle.DarkText;
                this.AddBindings (new Dictionary<object, string> ()                            
                {
                    { lblStatus, "{'Text':{'Path':'StatusInfoText'}}" },
                    { lblConfirmation, "{'Text':{'Path':'ConfirmationNoTxt'}}" },
					{ lblDriver, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.FullName'}, 'Hidden':{'Path':'VehicleDriverHidden'}}" },
					{ lblLicence, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.VehicleRegistration'}, 'Hidden':{'Path':'VehicleLicenceHidden'}}" },
					{ lblTaxiType, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.VehicleType'}, 'Hidden':{'Path':'VehicleTypeHidden'}}" },
					{ lblMake, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.VehicleMake'}, 'Hidden':{'Path':'VehicleMakeHidden'}}" },
					{ lblModel, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.VehicleModel'}, 'Hidden':{'Path':'VehicleModelHidden'}}" },
					{ lblColor, "{'Text':{'Path':'OrderStatusDetail.DriverInfos.VehicleColor'}, 'Hidden':{'Path':'VehicleColorHidden'}}" },
					{ txtDriver, "{'Hidden':{'Path':'VehicleDriverHidden'}}" },
					{ txtLicence, "{'Hidden':{'Path':'VehicleLicenceHidden'}}" },
					{ txtTaxiType, "{'Hidden':{'Path':'VehicleTypeHidden'}}" },
					{ txtMake, "{'Hidden':{'Path':'VehicleMakeHidden'}}" },
					{ txtModel, "{'Hidden':{'Path':'VehicleModelHidden'}}" },
					{ txtColor, "{'Hidden':{'Path':'VehicleColorHidden'}}" },
                    { statusBar, "{'IsEnabled':{'Path':'IsDriverInfoAvailable'}}" },
                    { imgGrip, "{'Hidden':{'Path':'IsDriverInfoAvailable', 'Converter':'BoolInverter'}}" },
                    { btnCallDriver, "{'TouchUpInside': {'Path': 'CallTaxi'}, 'Hidden':{'Path':'IsCallTaxiVisible', 'Converter':'BoolInverter'}}" },
				
                    { mapStatus, new B("Pickup","Pickup.Model")
                        .Add("TaxiLocation","OrderStatusDetail")
                        .Add("MapCenter","MapCenter") },

                    { btnCancel, new B("TouchUpInside","CancelOrder")
                        .Add("Hidden","IsCancelButtonVisible","BoolInverter")},

                    { btnPay, new B("TouchUpInside","PayForOrderCommand")
                        .Add("Hidden","IsPayButtonVisible","BoolInverter")},

                    { btnCall, new B("Hidden","IsCallButtonVisible","BoolInverter")
                        .Add("Enabled","IsCallButtonVisible")
                        .Add("TouchUpInside","CallCompany") },

                    { btnNewRide, new B("TouchUpInside","NewRide") }
					
                });
                mapStatus.Delegate = new AddressMapDelegate ();
                mapStatus.AddressSelectionMode = Data.AddressSelectionMode.None;

				UpdateTopSlidingStatus("OrderStatusDetail"); //initial loading
            
            } catch (Exception ex) {
                Logger.LogError (ex);
            }

            this.View.ApplyAppFont ();
        }

        void UpdateCallButtonSize (RectangleF callFrame)
        {
            if (!ViewModel.IsCancelButtonVisible && !ViewModel.IsPayButtonVisible) {
                btnCall.SetX ((View.Frame.Width - btnCancel.Frame.Width) / 2).SetWidth (btnCancel.Frame.Width);
                AppButtons.FormatStandardButton ((GradientButton)btnCall, Resources.StatusCallButton, AppStyle.ButtonColor.Black);
            }
            else {
                btnCall.SetFrame (callFrame);
            }
        }

		void UpdateTopSlidingStatus(string propertyName)
		{
			if (propertyName == "OrderStatusDetail") 
			{
				var numberOfItemsHidden = 0;
				var defaultHeightOfSlidingView = 130f;

				var tupleList = new List<Tuple<UILabel, UILabel>> ();
				tupleList.Add (Tuple.Create (lblTaxiType, txtTaxiType));
				tupleList.Add (Tuple.Create (lblMake, txtMake));
				tupleList.Add (Tuple.Create (lblModel, txtModel));
				tupleList.Add (Tuple.Create (lblColor, txtColor));

				if (ViewModel.VehicleDriverHidden){ 
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleLicenceHidden){ 
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleTypeHidden){ 
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleMakeHidden){ 
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleModelHidden){ 
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleColorHidden){ 
					numberOfItemsHidden++;
				}
	
				if (numberOfItemsHidden == 6) {
					statusBar.SetMaxHeight (topVisibleStatus.Frame.Height);
					return;
				}

				statusBar.SetMaxHeight (defaultHeightOfSlidingView - (20 * numberOfItemsHidden) + topVisibleStatus.Frame.Height);

				var i = 0;
				foreach (var item in tupleList) {
					if (!item.Item1.Hidden) {
						item.Item1.Frame = new RectangleF(item.Item1.Frame.X, 44 + (20 * i), item.Item1.Frame.Width, item.Item1.Frame.Height);
						item.Item2.Frame = new RectangleF(item.Item2.Frame.X, 44 + (20 * i), item.Item2.Frame.Width, item.Item2.Frame.Height);
						i++;
					}
				}
			}
		}
    }
}

