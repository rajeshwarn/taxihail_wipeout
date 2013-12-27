using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.MapUtilities;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
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

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            try {
                ViewModel.Load();

                NavigationItem.HidesBackButton = false;
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
                AppButtons.FormatStandardButton ((GradientButton)btnResend, Resources.GetValue ("ReSendConfirmationButton"), AppStyle.ButtonColor.Green);

                this.NavigationItem.TitleView = new TitleView (null, Resources.GenericTitle, true);
                                
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
                    btnResend.SetFrame(btnCancel.Frame.X, btnCancel.Frame.Y, btnResend.Frame.Width, btnResend.Frame.Height);

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

                    { btnResend, new B("Hidden","IsResendButtonVisible","BoolInverter")
                        .Add("Enabled","IsResendButtonVisible")
                            .Add("TouchUpInside","ResendConfirmationToDriver") },

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
            if (!ViewModel.IsCancelButtonVisible && !ViewModel.IsPayButtonVisible && !ViewModel.IsResendButtonVisible)
            {
                btnCall.SetX ((View.Frame.Width - btnCancel.Frame.Width) / 2).SetWidth (btnCancel.Frame.Width);
                AppButtons.FormatStandardButton ((GradientButton)btnCall, Resources.StatusCallButton, AppStyle.ButtonColor.Black);
            }
            else
            {
                btnCall.SetFrame (callFrame);
            }
        }

		void UpdateTopSlidingStatus(string propertyName)
		{
			if (propertyName == "OrderStatusDetail") 
			{
				var numberOfItemsHidden = 0;
				var defaultHeightOfSlidingView = 130f;

				var tupleList = new List<Tuple<UILabel, UILabel, bool>> ();
                tupleList.Add (Tuple.Create (lblDriver, txtDriver, false));
                tupleList.Add (Tuple.Create (lblLicence, txtLicence, false));
                tupleList.Add (Tuple.Create (lblTaxiType, txtTaxiType, false));
                tupleList.Add (Tuple.Create (lblMake, txtMake, false));
                tupleList.Add (Tuple.Create (lblModel, txtModel, false));
                tupleList.Add (Tuple.Create (lblColor, txtColor, false));

				if (ViewModel.VehicleDriverHidden){ 
                    tupleList[0] = Tuple.Create (tupleList[0].Item1, tupleList[0].Item2, true);
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleLicenceHidden){ 
                    tupleList[1] = Tuple.Create (tupleList[1].Item1, tupleList[1].Item2, true);
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleTypeHidden){ 
                    tupleList[2] = Tuple.Create (tupleList[2].Item1, tupleList[2].Item2, true);
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleMakeHidden){ 
                    tupleList[3] = Tuple.Create (tupleList[3].Item1, tupleList[3].Item2, true);
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleModelHidden){ 
                    tupleList[4] = Tuple.Create (tupleList[4].Item1, tupleList[4].Item2, true);
					numberOfItemsHidden++;
				}
				if (ViewModel.VehicleColorHidden){ 
                    tupleList[5] = Tuple.Create (tupleList[5].Item1, tupleList[5].Item2, true);
					numberOfItemsHidden++;
				}
	
				if (numberOfItemsHidden == 6) {
					statusBar.SetMaxHeight (topVisibleStatus.Frame.Height);
					return;
				}

				statusBar.SetMaxHeight (defaultHeightOfSlidingView - (20 * numberOfItemsHidden) + topVisibleStatus.Frame.Height);

				var i = 0;
				foreach (var item in tupleList) {
					if (!item.Item3) {
						item.Item1.Frame = new RectangleF(item.Item1.Frame.X, 4 + (20 * i), item.Item1.Frame.Width, item.Item1.Frame.Height);
						item.Item2.Frame = new RectangleF(item.Item2.Frame.X, 4 + (20 * i), item.Item2.Frame.Width, item.Item2.Frame.Height);
						i++;
					}
				}
			}
		}
    }
}

