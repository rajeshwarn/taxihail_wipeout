
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class ContactTaxiControl : BaseBindableChildView<BookingStatusViewModel>
    {
        public ContactTaxiControl (IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("ContactTaxiControl", null);

            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();

            this.DelayBind (() =>
            {
                InitializeBinding();
            });
        }

        private void Initialize()
        {
            view.BackgroundColor = Theme.CompanyColor;
            lblMedallionTitle.TextColor = Theme.LabelTextColor;
            lblMedallion.TextColor = Theme.LabelTextColor;

            btnCallDriver.SetImage(UIImage.FromFile("phone.png"), UIControlState.Normal);
        }

        private void InitializeBinding()
        {
            if (!ViewModel.IsCallTaxiVisible)
            {
                btnCallDriver.RemoveFromSuperview();
            }

            var set = this.CreateBindingSet<ContactTaxiControl, BookingStatusViewModel>();

            set.Bind(lblMedallion)
                .For(v => v.Text)
                .To(vm => vm.OrderStatusDetail.VehicleNumber);

            set.Bind(btnCallDriver)
                .For("TouchUpInside")
                .To(vm => vm.CallTaxi);

            set.Bind(btnCallDriver)
                .For(v => v.Hidden)
                .To(vm => vm.IsCallTaxiVisible)
                .WithConversion("BoolInverter");

            set.Apply();
        }

        private NSLayoutConstraint[] _hiddenContraints { get; set; }

        public bool HiddenWithConstraints
        {
            get
            {
                return base.Hidden;
            }
            set
            {
                if (base.Hidden != value)
                {
                    base.Hidden = value;
                    if (value)
                    {
                        _hiddenContraints = this.Superview.Constraints != null 
                            ? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
                            : null;
                        if (_hiddenContraints != null)
                        {
                            this.Superview.RemoveConstraints(_hiddenContraints);
                        }
                    }
                    else
                    {
                        if (_hiddenContraints != null)
                        {
                            this.Superview.AddConstraints(_hiddenContraints);
                            _hiddenContraints = null;
                        }
                    }
                }
            }
        }
    } 
}

