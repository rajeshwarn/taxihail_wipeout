
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class ChangeDropOffControl : BaseBindableChildView<BookingStatusViewModel>
    {
        public ChangeDropOffControl (IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("ChangeDropOffControl", null);

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
            lblChangeDropOff.TextColor = Theme.LabelTextColor;

            imgDropOffIcon.Image = UIImage.FromBundle("destination_small_icon_white");
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<ChangeDropOffControl, BookingStatusViewModel>();

            set.Bind(lblChangeDropOff)
                .For(v => v.Text)
                .To(vm => vm.ChangeDropOffText);

            set.Bind(btnAddOrRemoveDropOff)
                .To(vm => vm.AddOrRemoveDropOffCommand);

            set.Apply();
        }
    } 
}