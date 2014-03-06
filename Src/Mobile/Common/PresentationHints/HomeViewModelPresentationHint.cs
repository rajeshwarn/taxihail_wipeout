using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.PresentationHints
{

	public class HomeViewModelPresentationHint: ChangePresentationHint
    {
		public HomeViewModelPresentationHint(HomeViewModelState state)
        {
			State = state;
        }

        public HomeViewModelPresentationHint(HomeViewModelState state, bool newOrder)
        {
            State = state;
            IsNewOrder = newOrder;
        }

		public HomeViewModelState State
		{
			get;
			private set;
		}

        public bool IsNewOrder
        {
            get;
            set;
        }

    }
}

