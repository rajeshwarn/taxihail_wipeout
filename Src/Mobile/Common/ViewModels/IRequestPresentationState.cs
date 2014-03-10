using System;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public interface IRequestPresentationState<T>
	{
		event EventHandler<T> PresentationStateRequested;
	}

	public static class IRequestPresentationStateExtensions
	{
        public static void Raise<T>(this EventHandler<T> eh, object sender, T args) where T:EventArgs
		{
            if (eh != null)
			{
                eh.Invoke(sender, args);
			}
		}

        public static void Raise(this EventHandler eh, object sender, EventArgs args)
        {
            if (eh != null)
            {
                eh.Invoke(sender, args);
            }
        }

	}

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

