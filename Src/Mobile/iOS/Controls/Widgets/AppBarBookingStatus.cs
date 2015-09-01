using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class AppBarBookingStatus : BaseBindableChildView<BookingStatusBottomBarViewModel>
    {
        public AppBarBookingStatus (IntPtr handle) : base(handle)
        {
            
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("AppBarBookingStatus", null);

            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();

            this.DelayBind (InitializeBinding);
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            FlatButtonStyle.Red.ApplyTo(btnCancel);
            FlatButtonStyle.Red.ApplyTo(btnUnpair);
            FlatButtonStyle.Silver.ApplyTo(btnCall);
			FlatButtonStyle.Red.ApplyTo(btnUnpairFromRideLinq);
           
            var localize = this.Services().Localize;

            btnUnpairFromRideLinq.SetTitle(localize["UnpairPayInCar"], UIControlState.Normal);
            btnUnpair.SetTitle(localize["UnpairPayInCar"], UIControlState.Normal);
            btnCall.SetTitle(localize["CallButton"], UIControlState.Normal);
            btnCancel.SetTitle(localize["StatusCancelButton"], UIControlState.Normal);
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<AppBarBookingStatus, BookingStatusBottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelOrder);

            set.Bind(btnCancel)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsCancelButtonVisible)
                .WithConversion("BoolInverter");

            set.Bind(btnUnpair)
                .For(v => v.Command)
                .To(vm => vm.Unpair);

            set.Bind(btnUnpair)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsUnpairButtonVisible)
                .WithConversion("BoolInverter");

            set.Bind(btnCall)
                .For(v => v.Command)
                .To(vm => vm.CallCompany);

            set.Bind(btnCall)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.IsCallCompanyHidden);

			set.Bind (btnUnpairFromRideLinq)
				.For (v => v.Command)
				.To (vm => vm.UnpairFromRideLinq);

			set.Bind(btnUnpairFromRideLinq)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.IsUnpairFromManualRideLinqVisible)
				.WithConversion ("BoolInverter");

            set.Apply();
        }

        private NSLayoutConstraint[] _hiddenContraints { get; set; }

        public bool HiddenWithConstraints
        {
            get
            {
                return Hidden;
            }
            set
            {
                if (Hidden != value)
                {
                    Hidden = value;
                    if (value)
                    {
                        _hiddenContraints = Superview.Constraints != null 
                            ? Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
                            : null;
                        if (_hiddenContraints != null)
                        {
                            Superview.RemoveConstraints(_hiddenContraints);
                        }
                    }
                    else
                    {
                        if (_hiddenContraints != null)
                        {
                            Superview.AddConstraints(_hiddenContraints);
                            _hiddenContraints = null;
                        }
                    }
                }
            }
        }
    }
}

