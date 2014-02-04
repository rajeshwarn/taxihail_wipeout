using System;
using MonoTouch.MapKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Attributes;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public abstract class BindableMapView: MKMapView, IMvxBindable
    {
        public IMvxBindingContext BindingContext { get; set; }

        public BindableMapView()
        {
            this.CreateBindingContext();
        }

        public BindableMapView(IntPtr handle)
            : base(handle)
        {
            this.CreateBindingContext();
        }

        public BindableMapView(RectangleF frame)
            : base(frame)
        {
            this.CreateBindingContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BindingContext.ClearAllBindings();
            }
            base.Dispose(disposing);
        }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

    }
}

