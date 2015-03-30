using System;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderReviewView : BaseBindableChildView<OrderReviewViewModel>
    {
        public OrderReviewView(IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            txtNote.BackgroundColor = UIColor.FromRGB(242, 242, 242);
            txtNote.Font = UIFont.FromName(FontName.HelveticaNeueLight, 18f);
            txtNote.Placeholder = Localize.GetValue("NotesToDriveLabel");
            txtNote.ShowCloseButtonOnKeyboard();

			FlatButtonStyle.Silver.ApplyTo(btnViewPromo);
			btnViewPromo.Font = UIFont.FromName(FontName.HelveticaNeueRegular, 28 / 2);
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderReviewView, OrderReviewViewModel>();

            set.Bind(lblName)
                .For(v => v.Text)
                .To(vm => vm.Settings.Name);

            set.Bind(lblPhone)
                .For(v => v.Text)
                .To(vm => vm.Settings.Phone);

            set.BindSafe(lblNbPassengers)
                .For(v => v.Text)
                .To(vm => vm.Settings.Passengers);

            set.Bind(lblDate)
                .For(v => v.Text)
                .To(vm => vm.Date);

            set.Bind(lblVehicule)
                .For(v => v.Text)
                .To(vm => vm.VehiculeType);

            set.Bind(lblChargeType)
                .For(v => v.Text)
                .To(vm => vm.ChargeType);

            set.BindSafe(lblApt)
                .For(v => v.Text)
                .To(vm => vm.Apartment);

            set.BindSafe(lblRingCode)
                .For(v => v.Text)
                .To(vm => vm.RingCode);

            set.BindSafe(lblNbLargeBags)
                .For(v => v.Text)
                .To(vm => vm.Settings.LargeBags);

            set.Bind(txtNote)
				.For(v => v.Text)
                .To(vm => vm.Note);

			set.Bind(btnViewPromo)
				.For("TouchUpInside")
				.To(vm => vm.NavigateToPromotions);

			set.Bind (btnViewPromo)
				.For ("Title")
				.To (vm => vm.PromotionButtonText);

			set.Bind(iconPromo)
				.For(v => v.Hidden)
				.To(vm => vm.PromoCode)
				.WithConversion("HasValueToVisibility");

			if (!this.Services().Settings.ShowPassengerName)
            {
                lblName.RemoveFromSuperview();
                iconPassengerName.RemoveFromSuperview();
            }

			if (!this.Services().Settings.ShowPassengerNumber)
            {
                if (lblNbPassengers != null && iconNbPasserngers != null)
                {
                    lblNbPassengers.RemoveFromSuperview();
                    iconNbPasserngers.RemoveFromSuperview();
                }
            }

            if (!this.Services().Settings.ShowPassengerApartment)
            {
				lblApt.RemoveFromSuperview();
				iconApartment.RemoveFromSuperview ();
                lblRingCode.RemoveFromSuperview();
				iconRingCode.RemoveFromSuperview ();
            }

            if (!this.Services().Settings.ShowRingCodeField)
            {
                lblRingCode.RemoveFromSuperview();
				iconRingCode.RemoveFromSuperview();
            }

			if (!this.Services().Settings.ShowPassengerPhone)
            {
                lblPhone.RemoveFromSuperview();
                iconPhone.RemoveFromSuperview();
            }
                
            if (!this.Services().Settings.PromotionEnabled)
            {
				btnViewPromo.RemoveFromSuperview();
				iconPromo.RemoveFromSuperview();
            }

            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var nib = NibHelper.GetNibForView("OrderReviewView");
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            constraintHeight.Constant = this.Frame.Height;
        }
    }
}

