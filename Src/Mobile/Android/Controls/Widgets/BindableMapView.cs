using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Attributes;
using Android.Gms.Maps;
using Android.Content;
using Cirrious.CrossCore.Core;
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public abstract class BindableMapView: MapView, IMvxBindable
    {
        public IMvxBindingContext BindingContext { get; set; }

        public BindableMapView(Context context)
            : base(context)
        {
            this.CreateBindingContext();
        }

        public BindableMapView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            this.CreateBindingContext();
        }

        public BindableMapView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
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

