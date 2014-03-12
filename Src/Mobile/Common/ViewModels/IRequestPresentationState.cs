using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public interface IRequestPresentationState<T>
	{
		event EventHandler<T> PresentationStateRequested;
	}
}

