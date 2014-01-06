using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	public partial class CmtRideLinqConfirmPairView : BaseViewController<CmtRideLinqConfirmPairViewModel>
	{
		public CmtRideLinqConfirmPairView(MvxShowViewModelRequest request) : base(request)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			NavigationController.NavigationBar.Hidden = false;
			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

			var cancelButton = new MonoTouch.UIKit.UIBarButtonItem(Resources.GetValue("CmtCancelPayment"), UIBarButtonItemStyle.Plain, delegate {
				ViewModel.CancelPayment.Execute();
			});

			var confirmButton = new MonoTouch.UIKit.UIBarButtonItem(Resources.GetValue("CmtConfirmPayment"), UIBarButtonItemStyle.Plain, delegate {
				ViewModel.ConfirmPayment.Execute();
			});

			NavigationItem.HidesBackButton = true;
			NavigationItem.LeftBarButtonItem = cancelButton;
			NavigationItem.RightBarButtonItem = confirmButton;
			NavigationItem.Title = Resources.GetValue("CmtConfirmBookingInfo");

			lblCarNumber.Text= Resources.GetValue("CmtCarNumber");
			lblCardNumber.Text= Resources.GetValue("CmtCardNumber");
			lblTip.Text= Resources.GetValue("CmtTipAmount");

			AppButtons.FormatStandardButton((GradientButton)btnChangePaymentSettings, Resources.GetValue("CmtChangePaymentInfo"), AppStyle.ButtonColor.Silver );

			this.AddBindings(new Dictionary<object, string>{
				{ btnChangePaymentSettings, new B("TouchUpInside","ChangePaymentInfo") }, 
				{ lblCarNumberValue, new B("Text","CarNumber") }, 
				{ lblCardNumberValue, new B("Text","CardNumber") }, 
				{ lblTipValue, new B("Text","TipAmountInPercent") }, 
			});

			View.ApplyAppFont ();
		}
	}
}