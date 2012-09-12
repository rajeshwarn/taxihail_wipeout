using System;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Navigation
{
    public interface INavigationService
    {
        void Navigate<TViewModel,TView>() where TViewModel : BaseViewModel;
    }
}

