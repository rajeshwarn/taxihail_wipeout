using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableChildView<TViewModel> : MvxView
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

