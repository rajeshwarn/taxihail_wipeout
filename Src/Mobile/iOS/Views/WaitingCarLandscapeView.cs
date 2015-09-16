
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class WaitingCarLandscapeView : BaseViewController<WaitingCarLandscapeViewModel>
    {
        private const double LeftRotation = (Math.PI / 180) * 90;
		private const double RightRotation = (Math.PI / 180) * -90;

        public WaitingCarLandscapeView() : base("WaitingCarLandscapeView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Theme.CompanyColor;
			
            closeButton.SetTitle(Localize.GetValue("WaitingCarLandscapeViewCloseButtonText"), UIControlState.Normal);
            carNumberLabel.AccessibilityLabel = Localize.GetValue("WaitingCarLandscapeViewCarNumber");

            ViewModel_PropertyChanged(null, null);

            var bindSet = this.CreateBindingSet<WaitingCarLandscapeView, WaitingCarLandscapeViewModel>();

            bindSet.Bind(carNumberLabel)
                .For(e => e.Text)
                .To(vm => vm.CarNumber);

            bindSet.Bind(closeButton)
                .For("TouchUpInside")
                .To(vm => vm.CloseView);

            bindSet.Apply();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			if (ViewModel.DeviceOrientation == DeviceOrientation.Left)
			{
				mainView.Transform = CGAffineTransform.MakeRotation(new nfloat(LeftRotation));
			}
			else if (ViewModel.DeviceOrientation == DeviceOrientation.Right)
			{
				mainView.Transform = CGAffineTransform.MakeRotation(new nfloat(RightRotation));
			}
        }
    }
}