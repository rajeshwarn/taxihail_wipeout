using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public abstract class BaseBindableView<TViewModel> : MvxView
        where TViewModel : BaseViewModel
    {
        public BaseBindableView(IntPtr handle) : base(handle)
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

