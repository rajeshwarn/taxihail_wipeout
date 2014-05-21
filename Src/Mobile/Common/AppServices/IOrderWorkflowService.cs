using System;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IOrderWorkflowService
    {
		void PrepareForNewOrder();

		Task SetAddress(Address address);
		Task SetPickupAptAndRingCode(string apt, string ringCode);
		Task<Address> SetAddressToUserLocation();
		void ClearDestinationAddress();

        Task SetAddressToCoordinate(Position coordinate, CancellationToken cancellationToken);

		void SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		Task ValidatePickupDestinationAndTime();
		Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder();

		void SetBookingSettings(BookingSettings bookingSettings);
		Task SetAccountNumber (string accountNumber);
		void SetNoteToDriver(string text);

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();
		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();
		IObservable<BookingSettings> GetAndObserveBookingSettings();
		IObservable<DateTime?> GetAndObservePickupDate();
		IObservable<string> GetAndObserveEstimatedFare();
		IObservable<string> GetAndObserveNoteToDriver();
		IObservable<bool> GetAndObserveLoadingAddress();

		Task<bool> ShouldWarnAboutEstimate();

		Task<bool> ShouldGoToAccountNumberFlow();
		Task<bool> ValidateAccountNumberAndPrepareQuestions(string accountNumber = null);
		Task<AccountChargeQuestion[]> GetAccountPaymentQuestions();
        bool ValidateAndSaveAccountAnswers(AccountChargeQuestion[] questionsAndAnswers);

		Task<OrderValidationResult> ValidateOrder();
		void Rebook(Order previous);

		Task<Address> GetCurrentAddress();

		void ResetOrderSettings();
    }
}

