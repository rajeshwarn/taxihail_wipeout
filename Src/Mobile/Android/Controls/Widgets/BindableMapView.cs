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
    public abstract class BindableMapView: IMvxBindable
    {
        public IMvxBindingContext BindingContext { get; set; }

        public BindableMapView()
            : base()
        {
            this.CreateBindingContext();
        }
        // TODO: Dispose properly of resources inside OrderMapView.cs
//        protected override void Dispose(bool disposing)
//        {
//                 if (disposing)
//            {
//                BindingContext.ClearAllBindings();
//            }
//            base.Dispose(disposing);
//        }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

    }
}

