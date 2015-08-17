using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class BookingStatusControl : BaseBindableChildView<BookingStatusViewModel>
    {
        public BookingStatusControl (IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("BookingStatusControl", null);
            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();

            this.DelayBind (() =>
            {
                InitializeBinding();
            });
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<BookingStatusControl, BookingStatusViewModel>();

            set.Bind(lblOrderNumber)
                .For(v => v.Text)
                .To(vm => vm.ConfirmationNoTxt);

            set.Bind(lblOrderStatus)
                .For(v => v.Text)
                .To(vm => vm.StatusInfoText);

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

