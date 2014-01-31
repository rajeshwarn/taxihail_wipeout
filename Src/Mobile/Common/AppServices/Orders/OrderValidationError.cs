using System;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public enum OrderValidationError
    {
		DestinationAddressRequired,
		PickupAddressRequired,
		InvalidPickupDate,
    }
}

