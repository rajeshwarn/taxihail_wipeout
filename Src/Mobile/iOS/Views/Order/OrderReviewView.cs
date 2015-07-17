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
using apcurium.MK.Booking.Mobile.Client.Converters;
using Foundation;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderReviewView : BaseBindableChildView<OrderReviewViewModel>
    {
        // Offset to fix potential issue with iPhone 6+ that would not scroll the viewport completely.
        private const float ScrollingOffset = 3f;

        public OrderReviewView(IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            txtNote.BackgroundColor = UIColor.FromRGB(208, 208, 208);
            txtNote.Font = UIFont.FromName(FontName.HelveticaNeueBold, 18f);
            txtNote.Placeholder = Localize.GetValue("NotesToDriveLabel");
	        txtNote.PlaceholderColor = UIColor.FromRGB(75, 75, 75);
            txtNote.ShowCloseButtonOnKeyboard();

            Foundation.NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, ObserveKeyboardShown);

            FlatButtonStyle.CompanyColor.ApplyTo(btnViewPromo);
			btnViewPromo.Font = UIFont.FromName(FontName.HelveticaNeueRegular, 28 / 2);
        }
            
        // Places the visible area of the scrollviewer at the top of the driver note.
        private void ObserveKeyboardShown(NSNotification notification)
        {    
            var isKeyboardVisible = notification.Name == UIKeyboard.WillShowNotification;
            var keyboardFrame = isKeyboardVisible 
                ? UIKeyboard.FrameEndFromNotification(notification)
                : UIKeyboard.FrameBeginFromNotification(notification);

            var duration = UIKeyboard.AnimationDurationFromNotification(notification);

            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

            AnimateAsync(duration, async () => 
            {
                // We need to wait until the default animation from iOS stops before ajusting the scrollviewer to the correct location.
                await Task.Delay(1000);
                var activeView = KeyboardGetActiveView();
                if (activeView == null)
                {
                    return;
                }

                var scrollView = activeView.FindSuperviewOfType(this, typeof(UIScrollView)) as UIScrollView;
                if (scrollView == null)
                {
                    return;
                }

                var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardFrame.Height, 0.0f);
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;

                // Move the active field to the top of the active view area.
                var offset = activeView.Frame.Y - ScrollingOffset;
                scrollView.ContentOffset = new CoreGraphics.CGPoint(0, offset);
            });
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderReviewView, OrderReviewViewModel>();

            set.Bind(lblName)
                .For(v => v.Text)
                .To(vm => vm.Settings.Name);

            set.Bind(lblCountryDialCode)
                .For(v => v.Text)
                .To(vm => vm.Settings.Country)
                .WithConversion("DialCodeConverter");

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

            this.DelayBind(InitializeBinding);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            constraintHeight.Constant = this.Frame.Height;
        }
    }
}

