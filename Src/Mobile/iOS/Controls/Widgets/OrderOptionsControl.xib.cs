using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class OrderOptionsControl : BaseBindableView<HomeViewModel>
    {
        public OrderOptionsControl (IntPtr handle) : base(handle)
        {

        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            AddSubview(Line.CreateVertical(44f, 8f, 29f, UIColor.FromRGB(112, 112, 112), 1f));
            AddSubview(Line.CreateHorizontal(0f, 44f, Frame.Width, UIColor.FromRGB(178, 178, 178), 1f));
        }

        private void InitializeBinding()
        {

        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("OrderOptionsControl", null);
            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();
            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

