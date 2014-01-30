using System;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		Task SetPickupAddressToUserLocation();
		IObservable<Address> GetAndObservePickupAddress();
    }
}

