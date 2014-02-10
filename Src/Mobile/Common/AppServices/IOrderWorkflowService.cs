using System;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		Task SetAddress(Address address);
		Task SetAddressToUserLocation();
		Task ClearDestinationAddress();

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		Task ValidatePickupDestinationAndTime();

		Task SetBookingSettings(BookingSettings bookingSettings);

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();
		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();
		IObservable<BookingSettings> GetAndObserveBookingSettings();
		IObservable<DateTime?> GetAndObservePickupDate();

		IObservable<string> GetAndObserveEstimatedFare();
    }
}

