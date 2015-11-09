using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Enumeration;

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

            var bindSet = this.CreateBindingSet<WaitingCarLandscapeView, WaitingCarLandscapeViewModel>();

            bindSet.Bind(carNumberLabel)
                .For(e => e.Text)
                .To(vm => vm.CarNumber);

            bindSet.Bind(closeButton)
                .For("TouchUpInside")
                .To(vm => vm.CloseView);


            bindSet.Bind()
                .For(v => v.DeviceOrientation)
                .To(vm => vm.DeviceOrientation);

            bindSet.Apply();
        }


        DeviceOrientations _deviceOrientation;
        public DeviceOrientations DeviceOrientation
        {
            get
            {
                return _deviceOrientation;
            }
            set
            {
                if (_deviceOrientation == value)
                {
                    return;
                }

                _deviceOrientation = value;

                if (_deviceOrientation == DeviceOrientations.Left)
                {
                    mainView.Transform = CGAffineTransform.MakeRotation(new nfloat(LeftRotation));
                }
                else if (_deviceOrientation == DeviceOrientations.Right)
                {
                    mainView.Transform = CGAffineTransform.MakeRotation(new nfloat(RightRotation));
                }
            }
        }
    }
}