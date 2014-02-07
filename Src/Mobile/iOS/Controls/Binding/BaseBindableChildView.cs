using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableChildView<TViewModel> : OverlayView
        where TViewModel : ChildViewModel
    {
        public BaseBindableChildView(IntPtr handle) : base(handle)
        {
        }

        public new TViewModel ViewModel
        {
            get
            {
                return (TViewModel)DataContext;
            }
        }
    }
}

