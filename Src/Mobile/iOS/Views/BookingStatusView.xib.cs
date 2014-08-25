using System;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Style;


using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookingStatusView : BaseViewController<BookingStatusViewModel>
    {
		private const float DEFAULT_STATUS_LABEL_HEIGHT = 21f;
		private const float DEFAULT_TOP_VISIBLE_STATUS_HEIGHT = 45f;
        private float VisibleStatusHeight = 45f;

		public BookingStatusView () : base("BookingStatusView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue("View_BookingStatus");
            NavigationItem.HidesBackButton = false;

            ChangeThemeOfBarStyle();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            try 
			{
                View.BringSubviewToFront (statusBar);

                var textColor = UIColor.FromRGB (86, 86, 86);

                statusBar.Initialize ( topVisibleStatus, topSlidingStatus );
                lblConfirmation.Text = Localize.GetValue("LoadingMessage");
                txtDriver.Text = Localize.GetValue("DriverInfoDriver");
                txtDriver.TextColor = textColor;
                txtLicence.Text = Localize.GetValue("DriverInfoLicence");
                txtLicence.TextColor = textColor;
                txtTaxiType.Text = Localize.GetValue("DriverInfoTaxiType");
                txtTaxiType.TextColor = textColor;
                txtMake.Text = Localize.GetValue("DriverInfoMake");
                txtMake.TextColor = textColor;
                txtModel.Text = Localize.GetValue("DriverInfoModel");
                txtModel.TextColor = textColor;
                txtColor.Text = Localize.GetValue("DriverInfoColor");
                txtColor.TextColor = textColor;

                topSlidingStatus.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("background.png"));
                topVisibleStatus.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("backPickupDestination.png"));

                viewLine.Frame = new RectangleF(0, topSlidingStatus.Bounds.Height -1, topSlidingStatus.Bounds.Width, 1);

                btnCallDriver.SetImage(UIImage.FromFile("phone.png"), UIControlState.Normal);
                btnCall.SetTitle(Localize.GetValue("StatusCallButton"), UIControlState.Normal);
                btnCancel.SetTitle(Localize.GetValue("StatusCancelButton"), UIControlState.Normal);
                btnNewRide.SetTitle(Localize.GetValue("StatusNewRideButton"), UIControlState.Normal);
                btnPay.SetTitle(Localize.GetValue("PayNow"), UIControlState.Normal);
                btnResend.SetTitle(Localize.GetValue("ReSendConfirmation"), UIControlState.Normal);
                btnUnpair.SetTitle(Localize.GetValue("CmtRideLinqUnpair"), UIControlState.Normal);

                FlatButtonStyle.Silver.ApplyTo(btnCallDriver);
                FlatButtonStyle.Silver.ApplyTo(btnCall);
                FlatButtonStyle.Red.ApplyTo(btnCancel);
                FlatButtonStyle.Green.ApplyTo(btnNewRide);
                FlatButtonStyle.Green.ApplyTo(btnPay);
                FlatButtonStyle.Green.ApplyTo(btnResend);
                FlatButtonStyle.Red.ApplyTo(btnUnpair);
                                                
                View.BringSubviewToFront (bottomBar);

				ViewModel.PropertyChanged+= (sender, e) => {
					InvokeOnMainThread(()=>
					{
						UpdateTopSlidingStatus(e.PropertyName);
					});
				};

				if(!ViewModel.Settings.HideCallDispatchButton)
                {
                    btnCancel.SetFrame(8, btnCancel.Frame.Y,  btnCancel.Frame.Width,  btnCancel.Frame.Height );
                    btnCall.SetFrame( 320 - 8 - btnCall.Frame.Width ,  btnCall.Frame.Y,  btnCall.Frame.Width,  btnCall.Frame.Height );
                    btnPay.SetFrame(btnCancel.Frame);
                    btnResend.SetFrame(btnCancel.Frame.X, btnCancel.Frame.Y, btnResend.Frame.Width, btnResend.Frame.Height);
					btnUnpair.SetFrame(btnCancel.Frame.X, btnCancel.Frame.Y, btnUnpair.Frame.Width, btnUnpair.Frame.Height);

                    var callFrame = btnCall.Frame;
                    UpdateCallButtonSize (callFrame);
                    ViewModel.PropertyChanged += (sender, e) => 
                    {
                        InvokeOnMainThread(()=>
                        {
                            UpdateCallButtonSize (callFrame);
                        });
                    };
                }

                textColor = UIColor.FromRGB (50, 50, 50);

                lblDriver.TextColor = textColor;
                lblLicence.TextColor = textColor;
                lblTaxiType.TextColor = textColor;
                lblMake.TextColor = textColor;
                lblModel.TextColor = textColor;
                lblColor.TextColor = textColor;

                lblConfirmation.TextColor = textColor;
                lblStatus.TextColor = textColor;

                var set = this.CreateBindingSet<BookingStatusView, BookingStatusViewModel>();

                set.Bind(this)
                    .For("StatusInfoText")
                    .To(vm => vm.StatusInfoText);

                set.Bind(lblConfirmation)
                    .For(v => v.Text)
                    .To(vm => vm.ConfirmationNoTxt);

                set.Bind(lblDriver)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.FullName);
                set.Bind(lblDriver)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleDriverHidden);

                set.Bind(lblLicence)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.VehicleRegistration);
                set.Bind(lblLicence)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleLicenceHidden);

                set.Bind(lblTaxiType)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.VehicleType);
                set.Bind(lblTaxiType)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleTypeHidden);

                set.Bind(lblMake)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.VehicleMake);
                set.Bind(lblMake)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleMakeHidden);

                set.Bind(lblModel)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.VehicleModel);
                set.Bind(lblModel)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleModelHidden);

                set.Bind(lblColor)
                    .For(v => v.Text)
                    .To(vm => vm.OrderStatusDetail.DriverInfos.VehicleColor);
                set.Bind(lblColor)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleColorHidden);

                set.Bind(txtDriver)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleDriverHidden);

                set.Bind(txtLicence)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleLicenceHidden);

                set.Bind(txtTaxiType)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleTypeHidden);

                set.Bind(txtMake)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleMakeHidden);

                set.Bind(txtModel)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleModelHidden);

                set.Bind(txtColor)
                    .For(v => v.Hidden)
                    .To(vm => vm.VehicleColorHidden);

                set.Bind(statusBar)
                    .For(v => v.IsEnabled)
                    .To(vm => vm.IsDriverInfoAvailable);

                set.Bind(imgGrip)
                    .For(v => v.Hidden)
                    .To(vm => vm.IsDriverInfoAvailable)
                    .WithConversion("BoolInverter");
                    
                set.Bind(btnCallDriver)
                    .For("TouchUpInside")
                    .To(vm => vm.CallTaxi);
                set.Bind(btnCallDriver)
                    .For(v => v.Hidden)
                    .To(vm => vm.IsCallTaxiVisible)
                    .WithConversion("BoolInverter");

				set.Bind(mapStatus)
					.For(v => v.Pickup)
                    .To(vm => vm.Order.PickupAddress);
                set.Bind(mapStatus)
                    .For(v => v.Dropoff)
                    .To(vm => vm.Order.DropOffAddress);
				set.Bind(mapStatus)
					.For(v => v.TaxiLocation)
					.To(vm => vm.OrderStatusDetail);
				set.Bind(mapStatus)
					.For(v => v.MapCenter)
					.To(vm => vm.MapCenter);

				set.Bind(btnCancel)
					.For("TouchUpInside")
					.To(vm => vm.CancelOrder);
				set.Bind(btnCancel)
					.For(v => v.Hidden)
					.To(vm => vm.IsCancelButtonVisible)
					.WithConversion("BoolInverter");

				set.Bind(btnPay)
					.For("TouchUpInside")
					.To(vm => vm.PayForOrderCommand);
				set.Bind(btnPay)
					.For(v => v.Hidden)
					.To(vm => vm.IsPayButtonVisible)
					.WithConversion("BoolInverter");

				set.Bind(btnCall)
					.For(v => v.Hidden)
					.To(vm => vm.Settings.HideCallDispatchButton);
				set.Bind(btnCall)
					.For(v => v.Enabled)
					.To(vm => vm.Settings.HideCallDispatchButton)
					.WithConversion("BoolInverter");
				set.Bind(btnCall)
					.For("TouchUpInside")
					.To(vm => vm.CallCompany);

				set.Bind(btnResend)
					.For(v => v.Hidden)
					.To(vm => vm.IsResendButtonVisible)
					.WithConversion("BoolInverter");
				set.Bind(btnResend)
					.For(v => v.Enabled)
					.To(vm => vm.IsResendButtonVisible);
				set.Bind(btnResend)
					.For("TouchUpInside")
					.To(vm => vm.ResendConfirmationToDriver);

				set.Bind(btnNewRide)
					.For("TouchUpInside")
					.To(vm => vm.NewRide);

				set.Bind(btnUnpair)
					.For("TouchUpInside")
					.To(vm => vm.Unpair);
				set.Bind(btnUnpair)
					.For(v => v.Hidden)
					.To(vm => vm.IsUnpairButtonVisible)
					.WithConversion("BoolInverter");

				set.Apply();

                mapStatus.Delegate = new AddressMapDelegate ();
                mapStatus.AddressSelectionMode = AddressSelectionMode.None;

				UpdateTopSlidingStatus("OrderStatusDetail"); //initial loading
                var statusLineDivider = Line.CreateHorizontal(320.0f, UIColor.Black.ColorWithAlpha(0.35f));
                bottomBar.AddSubview(statusLineDivider);
            
            } 
			catch (Exception ex) 
			{
                Logger.LogError (ex);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (IsMovingFromParentViewController)
            {
                // Back button pressed
                ViewModel.PrepareNewOrder.Execute(null);
            }
        }

        void UpdateCallButtonSize (RectangleF callFrame)
        {
            if (!ViewModel.IsCancelButtonVisible && !ViewModel.IsPayButtonVisible && !ViewModel.IsResendButtonVisible)
            {
                btnCall.SetX ((View.Frame.Width - btnCancel.Frame.Width) / 2).SetWidth (btnCancel.Frame.Width);
                btnCall.SetTitle(Localize.GetValue("StatusCallButton"), UIControlState.Normal);
                FlatButtonStyle.Silver.ApplyTo(btnCall);
            }
            else
            {
                btnCall.SetFrame (callFrame);
            }
        }

		public string StatusInfoText
		{
			set
			{
				if(lblStatus.Text != value)
				{
					lblStatus.Text = value;
					var nbLines = 1 + (int)(lblStatus.StringSize(value, lblStatus.Font).Width / lblStatus.Frame.Width);
                    var togglePadding = DEFAULT_STATUS_LABEL_HEIGHT * (nbLines - 1);
                    lblStatus.SetHeight (DEFAULT_STATUS_LABEL_HEIGHT + togglePadding);              // increase label height
                    VisibleStatusHeight = DEFAULT_TOP_VISIBLE_STATUS_HEIGHT + togglePadding;
                    statusBar.SetMinHeight (VisibleStatusHeight);
                    statusBar.SetMaxHeight (VisibleStatusHeight);
				}
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
                    statusBar.SetMaxHeight (VisibleStatusHeight);
					return;
				}

                statusBar.SetMaxHeight (defaultHeightOfSlidingView - (20 * numberOfItemsHidden) + VisibleStatusHeight);

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

