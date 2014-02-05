using System;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class HomeViewModelPresentationHint: MvxPresentationHint
    {
		public HomeViewModelPresentationHint(HomeViewModelState state)
        {
			State = state;
        }

		public HomeViewModelState State
		{
			get;
			private set;
		}
    }
}

