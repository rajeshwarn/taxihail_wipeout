using System;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public interface IRequestPresentationState<T>
	{
		event EventHandler<T> PresentationStateRequested;
	}
}

