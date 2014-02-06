using System;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		Task SetAddressToUserLocation();
		Task ClearDestinationAddress();

        Task SetAddressToCoordinate(Position userMapBoundsCoordinate);

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		Task ValidatePickupDestinationAndTime();

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();

		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();

		IObservable<string> GetAndObserveEstimatedFare();
    }
}

