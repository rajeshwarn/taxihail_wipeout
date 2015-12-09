using System;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class AppBarDropOffSelection : BaseBindableChildView<DropOffSelectionBottomBarViewModel>
    {
        public AppBarDropOffSelection (IntPtr handle) : base(handle)
        {

        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("AppBarDropOffSelection", null);
            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();

            this.DelayBind (InitializeBinding);
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            FlatButtonStyle.Red.ApplyTo(btnCancel);
            FlatButtonStyle.Green.ApplyTo(btnOk);

            var localize = this.Services().Localize;

            btnCancel.SetTitle(localize["Cancel"], UIControlState.Normal);
            btnOk.SetTitle(localize["OkButtonText"], UIControlState.Normal);
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<AppBarDropOffSelection, DropOffSelectionBottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.Cancel);

            set.Bind(btnOk)
                .For(v => v.Command)
                .To(vm => vm.SaveDropOff);

            set.Apply();
        }
    }
}

