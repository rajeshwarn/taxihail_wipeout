using System;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		Task SetAddress(Address address);
		void SetPickupAddress(Address address);
		Task<Address> SetAddressToUserLocation();
		Task ClearDestinationAddress();

        Task SetAddressToCoordinate(Position userMapBoundsCoordinate, CancellationToken cancellationToken);

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		Task ValidatePickupDestinationAndTime();
		Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder();

		Task SetBookingSettings(BookingSettings bookingSettings);

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();
		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();
		IObservable<BookingSettings> GetAndObserveBookingSettings();
		IObservable<DateTime?> GetAndObservePickupDate();
		IObservable<string> GetAndObserveEstimatedFare();

		void SetNoteToDriver(string text);
		Task<bool> ShouldWarnAboutEstimate();

		Task<OrderValidationResult> ValidateOrder();
		void Rebook(Order previous);

		Task<Address> GetCurrentAddress();
    }
}

