using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderReviewView : OverlayView
    {
        private NSLayoutConstraint _heightConstraint;

        public OrderReviewView(IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            _heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, 
                NSLayoutRelation.Equal, 
                null, 
                NSLayoutAttribute.NoAttribute, 
                1.0f, 300.0f);

            this.AddConstraint(_heightConstraint);
            BackgroundColor = UIColor.Clear;
            txtNote.BackgroundColor = new UIColor();
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

            set.Bind(lblNbPassengers)
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

            set.Bind(lblApt)
                .For(v => v.Text)
                .To(vm => vm.Apartment);

            set.Bind(lblRingCode)
                .For(v => v.Text)
                .To(vm => vm.RingCode);



            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var nib = UINib.FromName ("OrderReviewView", null);
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

