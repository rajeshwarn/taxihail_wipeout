using System;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		Task SetAddress(Address address);
		Task SetAddressToUserLocation();
		Task ClearDestinationAddress();

        Task SetAddressToCoordinate(Position userMapBoundsCoordinate, CancellationToken cancellationToken);

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		Task ValidatePickupDestinationAndTime();

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();
		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();
		IObservable<BookingSettings> GetAndObserveBookingSettings();

		IObservable<string> GetAndObserveEstimatedFare();
    }
}

