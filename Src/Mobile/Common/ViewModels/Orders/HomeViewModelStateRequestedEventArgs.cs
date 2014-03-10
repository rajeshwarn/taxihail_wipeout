using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{

	public class HomeViewModelStateRequestedEventArgs: EventArgs
	{
        public HomeViewModelStateRequestedEventArgs(HomeViewModelState state, bool isNewOrder)
			:this(state)
		{
			this.IsNewOrder = isNewOrder;
			
		}

		public HomeViewModelStateRequestedEventArgs(HomeViewModelState state)
		{
			this.State = state;
		}

		public bool IsNewOrder
		{
			get;
			private set;
		}

		public HomeViewModelState State
		{
			get;
			private set;
		}
	}
}
