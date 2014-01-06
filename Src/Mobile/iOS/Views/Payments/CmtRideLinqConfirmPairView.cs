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

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public class CmtRideLinqConfirmPairView : BaseViewController<CmtRideLinqConfirmPairViewModel>
	{
		public CmtRideLinqConfirmPairView(MvxShowViewModelRequest request) : base(request)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			lblConfirmDriverInfo.Text = Resources.GetValue("VehicleNumberInfo");
			lblConfirmDriverNotice.Text = Resources.GetValue("VehicleNumberNotice");
			lblConfirmDriverNotice.Font = AppStyle.GetNormalFont (13);
		}

		public override void ViewWillAppear (bool animated)
		{
			this.NavigationController.NavigationBarHidden = false;
			base.ViewWillAppear (animated);
		}
	}
}