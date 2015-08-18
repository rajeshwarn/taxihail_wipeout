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
        private NSLayoutConstraint _heightConstraint;

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
            //_heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 60.0f);
            //AddConstraint(_heightConstraint);
            //lblOrderNumber.Text = "Order #12345";
            //lblOrderStatus.Text = "this is le status asklfsadf nadsfinasdi a sdf";
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

        public void Resize()
        {
            _heightConstraint.Constant = (nfloat)Subviews[0].Subviews.Where(x => !x.Hidden).Sum(x => x.Frame.Height);
            SetNeedsDisplay();
        }
    } 
}

