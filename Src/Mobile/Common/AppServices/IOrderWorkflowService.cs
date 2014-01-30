using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		void SetPickupAddressToUserLocation();
		IObservable<Address> GetAndObservePickupAddress();
    }
}

