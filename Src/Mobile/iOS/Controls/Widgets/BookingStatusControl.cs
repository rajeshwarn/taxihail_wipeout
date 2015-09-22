using System;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;

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

            this.DelayBind (InitializeBinding);
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;

            // Create rounded driver picture
            driverPhoto.Layer.CornerRadius = driverPhoto.Frame.Size.Width / 2;
            driverPhoto.ClipsToBounds = true;
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<BookingStatusControl, BookingStatusViewModel>();

            set.Bind(lblOrderNumber)
                .For(v => v.Text)
                .To(vm => vm.ConfirmationNoTxt);

			set.Bind(lblOrderNumber)
				.For(v => v.Hidden)
				.To(vm => vm.IsConfirmationNoHidden);

            set.Bind(lblOrderStatus)
                .For(v => v.Text)
                .To(vm => vm.StatusInfoText);

            set.Bind(viewDriverInfos)
                .For(v => v.Hidden)
                .To(vm => vm.IsDriverInfoAvailable)
                .WithConversion("BoolInverter");

            set.Bind(driverPhoto)
                .For(v => v.ImageUrl)
                .To(vm => vm.OrderStatusDetail.DriverInfos.DriverPhotoUrl);

            set.Bind(lblDriverName)
                .For(v => v.Text)
                .To(vm => vm.OrderStatusDetail.DriverInfos.FullName);

            set.Bind(lblVehicleInfos)
                .For(v => v.Text)
                .To(vm => vm.OrderStatusDetail.DriverInfos.FullVehicleInfo);

            set.Apply();
        }
    } 
}